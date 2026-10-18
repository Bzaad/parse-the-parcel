# Parse the Parcel — Design Document

## 1. Purpose

Trade Me is launching a national package shipping network with flat rates by box size. This component takes a parcel's dimensions and weight and returns the **package type and cost**, or **no solution** if the parcel is too large or too heavy. The website will use it to show sellers what shipping will cost.

Source: *Parse the Parcel.pdf* (the brief). Decisions marked **[Assumption]** are not stated in the brief; they are inferred from the existing code and README and are open to change.

## 2. Requirements

### From the brief

| Package | Length | Breadth | Height | Cost |
|---|---|---|---|---|
| Small | 200mm | 300mm | 150mm | $5.00 |
| Medium | 300mm | 400mm | 200mm | $7.50 |
| Large | 400mm | 600mm | 250mm | $8.50 |

- Maximum weight is 25kg per package.
- If the parcel exceeds the dimensions of every package, or is over 25kg, return no packaging solution.
- Provide a runnable entry point (CLI, simple API, etc.), with clear separation of concerns.
- Tests for main cases and edge cases: oversized, overweight, boundary dimensions.
- Readable code and sensible handling of invalid input.
- Keep scope narrow (2–3 hours of work).

### Added requirement: API project

A new project, **`ParcelShipping.Api`**, is added under `src`. It is the runnable entry point and exposes **one `POST` endpoint**. The caller sends a parcel's dimensions and weight and gets one of three outcomes:

| Outcome | Status | Response |
|---|---|---|
| A suitable box is found | `200 OK` | Package type and price |
| Valid parcel, but overweight or oversized | `422 Unprocessable Entity` | A message |
| Request has a zero or negative value | `400 Bad Request` | Error |

### Non-goals

Pricing by weight, multi-parcel orders, packing several items into one box, courier routing, currency/tax, authentication, persistence.

## 3. Assumptions

1. **[Assumption] The table is the maximum parcel size per box.** A parcel exactly at a limit fits, since the brief says "exceeds".
2. **[Assumption] Rotation is allowed.** Length, breadth and height are interchangeable, so 400 × 300 × 200 fits Medium.
3. **[Assumption] Exactly 25kg is accepted.** Only weight strictly over 25kg is rejected.
4. **[Assumption] Weight is only a cutoff.** It never changes which box is chosen.
5. **[Assumption] No minimum size.** Any positive-sized parcel fits Small at least.
6. **[Assumption] Invalid input is an error, not "no solution".** Zero, negative or non-numeric values are rejected. "No solution" is for valid parcels that are too big or heavy.
7. **[Assumption] Units:** millimetres and kilograms; decimals allowed.
8. **[Assumption] The cheapest fitting box wins.** With the current table this is also the smallest, but the logic does not rely on that.

## 4. Architecture

```
┌──────────────────────────┐
│  ParcelShipping.Api      │  Thin Web API (src): one POST endpoint, request/response
│                          │  DTOs, maps result/exception -> 200 / 422 / 400
└────────────┬─────────────┘
             │ depends on
┌────────────▼─────────────┐
│  ParcelShipping.Core     │  Domain logic, no knowledge of callers
│  Parcel                  │
│  PackageSelector         │──► IPackageCatalogue ◄── InCodePackageCatalogue
│  PackageDefinition       │
│  PackagingResult         │
└──────────────────────────┘
┌──────────────────────────┐
│  ParcelShipping.Tests    │  xUnit tests: Core units, controller, HTTP endpoint
└──────────────────────────┘
```

Targets .NET 10. Dependencies point inward: the API references Core; Core references nothing. Request/response DTOs live in the API layer only.

**Status:** Core, Api and Tests are implemented. `ParcelShipping.Api` is the entry point the brief asks for ("something we could actually run"). `src/ParcelShipping.Api/ParcelShipping.Api.http` has sample requests for every outcome, and `launchSettings.json` fixes the URL at `http://localhost:5080`.

## 5. Components (Core)

Files in `src/ParcelShipping.Core` are grouped by role. Every type stays in the single `ParcelShipping.Core` namespace, so callers need one `using`.

| Folder | Contents |
|---|---|
| `Models/` | `Parcel`, `PackageDefinition`, `PackagingResult`, `PackageType` |
| `Abstractions/` | `IPackageCatalogue` |
| `Catalogue/` | `InCodePackageCatalogue` |
| `Services/` | `PackageSelector` |

| Type | Responsibility |
|---|---|
| `Parcel` | Immutable record of length, breadth, height (mm) and weight (kg). The constructor rejects any value ≤ 0 with `ArgumentOutOfRangeException` naming the argument, so an invalid parcel cannot exist. |
| `PackageType` | Enum: `Small`, `Medium`, `Large`. |
| `PackageDefinition` | A box's maximum size and cost, plus `Fits(Parcel)`. |
| `IPackageCatalogue` | `GetPackages()` — the source of box definitions. |
| `InCodePackageCatalogue` | First implementation: the brief's table as a static list. |
| `PackagingResult` | `(PackageType, Cost)`. Always a real box and cost. |
| `PackageSelector` | `Select(Parcel)` — applies the weight cutoff and picks the cheapest box that fits. |

### Selection algorithm

```
Select(parcel):
    if parcel.Weight > 25kg:            return null
    package = catalogue.GetPackages()
                .OrderBy(cost)
                .FirstOrDefault(p => p.Fits(parcel))
    return package == null ? null : new PackagingResult(package.Type, package.Cost)
```

### Fit check

Sort the parcel's three sides and the box's three sides ascending, then require `parcel[i] <= box[i]` for each pair. This handles rotation and exact fits.

A volume comparison was rejected: a 300 × 300 × 300mm parcel has less volume than Large (400 × 600 × 250) but its smallest side (300) exceeds Large's smallest (250), so it does not fit. The sorted-sides check rejects it correctly.

## 6. Key Design Decisions

| Decision | Choice | Alternatives and trade-off |
|---|---|---|
| "No solution" | `null` `PackagingResult?` | A `NotFound` enum value would allow nonsense states such as "NotFound, $5.00"; a `TryX` method is clumsy from a controller. Cost of `null`: callers must check, mitigated by nullable reference types and mapping to a clear API response. |
| Invalid input | Throws in `Parcel` constructor | Returning "no solution" would hide caller bugs. The API layer translates it to a 400 response. |
| Box data | Behind `IPackageCatalogue` | Prices and sizes can change without touching selection logic. Moving to `appsettings.json` or a database changes one class. No configuration infrastructure is built now. |
| Money and dimensions | `decimal` | Avoids floating-point error at boundaries (e.g. 200.0 vs 199.99999) and in prices. |
| Cheapest box by cost, not by order | `OrderBy(Cost)` in selector | Does not depend on catalogue ordering; covered by a test. |
| DTOs | API layer only | Keeps Core free of transport concerns. |

## 7. Error Handling

| Situation | Behaviour |
|---|---|
| Zero or negative dimension/weight | `ArgumentOutOfRangeException` from `Parcel`; API returns **400** |
| Non-numeric or missing input | Rejected at the API boundary during model binding; **400** |
| Valid parcel, over 25kg | `null`; API returns **422** with a message |
| Valid parcel, too big for every box | `null`; API returns **422** with a message |

Open point: `Select` does not null-check the `parcel` argument or the catalogue passed to the constructor. Worth adding guard clauses when finishing the code.

## 8. Testing

Implemented (xUnit, in `ParcelShipping.Tests`):

- `ParcelTests`: values stored correctly; zero/negative on each of the four fields throws naming the argument; tiny positive values accepted.
- `PackageSelectorTests`: each box chosen for a fitting parcel, including exact boundaries, 1mm over and rotated variants; oversized in every box returns null; exactly 25kg accepted; just over 25kg returns null; catalogue not sorted by cost still gives the cheapest fit; weight below the limit does not change the box.
- `Api/PackagesControllerTests`: calls the controller directly for all three outcomes (200, 422 for oversized and overweight, 400 naming the field for zero and negative values).
- `Api/QuoteEndpointTests`: `WebApplicationFactory` tests over HTTP for 200, 422, 400 for zero and negative values, and 400 for non-numeric and missing fields.

Planned:

- The 300 × 300 × 300 counterexample, which pins the sorted-sides logic.
- `PackageDefinition.Fits` direct tests.

Broader integration tests are out of scope.

## 9. API: `ParcelShipping.Api`

A web project at `src/ParcelShipping.Api`, referencing `ParcelShipping.Core`. It has **one endpoint**.

`POST /api/packages/quote` **[Assumption: route name]**

Request:
```json
{ "length": 250, "breadth": 350, "height": 180, "weight": 3.2 }
```
Dimensions in mm, weight in kg.

Responses:

| Status | When | Body |
|---|---|---|
| 200 | A box fits | `{ "packageType": 1, "cost": 7.50 }` |
| 422 | Valid parcel, overweight or oversized | `{ "message": "..." }` |
| 400 | Zero or negative value (or missing/non-numeric field) | Error naming the offending field |

`packageType` is serialised as the enum's number (`0` Small, `1` Medium, `2` Large).

Flow: bind the request DTO → build a `Parcel` (invalid values throw, caught and returned as 400) → `PackageSelector.Select` → `null` becomes 422, otherwise 200.

Notes:
- `PackageSelector` and `InCodePackageCatalogue` are registered in dependency injection.
- **[Assumption]** Overweight and oversized share one 422 message, because Core returns `null` for both and does not say which. If the caller needs to know the reason, `PackagingResult` or a new outcome type in Core would have to carry it.
- **[Assumption]** The 400 body uses the ASP.NET validation problem details shape (`title`, `status`, `errors`). For zero or negative values the controller builds it itself and the error text is the .NET exception message, which is untidy (it includes the parameter name and actual value on a second line). Tidying that text is an open item.
- DTOs live in the API project only; Core stays unaware of HTTP.

## 10. Risks and Open Questions

1. Is the table the box's maximum parcel size or its outside dimensions? (Assumption 1.)
2. Can a parcel be rotated? If the courier requires a fixed orientation, the fit check changes to a direct comparison.
3. Should there be a minimum size or weight?
4. Is a heavier parcel ever priced differently within the same box?
5. Should the 422 message say whether the parcel is overweight or oversized? (Currently one shared message.)
6. Should `packageType` be returned as a name (`"Medium"`) rather than a number?
7. Should the 400 error text be a plain message instead of the exception text?

## 11. Future Extensions

- Load the catalogue from `appsettings.json` with reload-on-change, or a database.
- Add a CLI alongside the API, both using Core.
- Further box types, or per-weight surcharges, by extending `PackageDefinition` and the selector.
- Logging and metrics on "no solution" rates to inform new box sizes.

## 12. Delivery Plan

1. Core library and unit tests — **done**.
2. `src/ParcelShipping.Api` with its single POST endpoint, DTOs, and the 200 / 422 / 400 mapping — **done**.
3. API tests (controller and HTTP) and an `.http` file of sample requests — **done**; all 56 tests pass and the `.http` requests were run against the live API.
4. README run instructions — **next**.
5. Pull request.

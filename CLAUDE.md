# CLAUDE.md

The 'Parse the Parcel' exercise: a component that takes a parcel's dimensions and weight and returns the package type and cost, or no solution. Read [DESIGN.md](DESIGN.md) for requirements, assumptions and decisions, and [README.md](README.md) for the assumptions list.

## Layout

- `src/ParcelShipping.Core` — domain logic. No knowledge of HTTP or any caller.
- `src/ParcelShipping.Api` — thin Web API: one POST endpoint, DTOs, maps results to 200 / 422 / 400.
- `tests/ParcelShipping.Tests` — xUnit tests.

Targets .NET 10.

## Commands

- Build: `dotnet build`
- Test: `dotnet test`
- Run the API: `dotnet run --project src/ParcelShipping.Api` (listens on `http://localhost:5080`)
- Try it: send the requests in `src/ParcelShipping.Api/ParcelShipping.Api.http`

## Conventions

- Dependencies point inward: Api references Core; Core references nothing.
- Request/response DTOs live in the Api project only. Core uses its own `Parcel` type.
- Keep nullable reference types enabled. "No solution" is a `null` `PackagingResult`; invalid input throws from the `Parcel` constructor.
- Use `decimal` for dimensions, weight and money.
- Box sizes and prices come from `IPackageCatalogue`, not hard-coded in the selector.
- Fit is checked by comparing sorted sides, not volume. Do not change this to a volume check.
- Keep scope narrow. Do not add features the brief does not ask for.

## Testing

- Add or update tests with every behaviour change.
- Cover each box type, oversized, overweight, exact boundaries (and 1mm over), exactly 25kg, rotation and invalid input.
- API tests cover all three outcomes: 200, 422 (overweight and oversized) and 400 (zero and negative values), both by calling the controller directly and over HTTP with `WebApplicationFactory`.
- When the API changes, keep the `.http` file in step with it.

## Working rules

- If you change a behaviour or assumption, update DESIGN.md and README.md to match.
- Write small commits with a short imperative subject and a body explaining why.
- Show the commit message before committing unless told to go ahead.
- Do not commit `.claude/` or the brief PDF unless asked.

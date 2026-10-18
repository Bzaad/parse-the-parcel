# parse-the-parcel
The 'Parse the Parcel' exercise for the Trade Me technical job interview.

## Assumptions

These are my readings of the brief where it was ambiguous. Each one is open to change if the requirements say otherwise.

1. **The table gives the maximum parcel size.** The sizes are the largest parcel each type accepts, not the box's outside dimensions. A parcel of exactly 300mm fits the Medium box, because the brief says a parcel is rejected only if it *exceeds* the dimensions.
2. **Rotation is allowed.** The parcel can be turned to fit, so length, breadth and height are interchangeable. A 400 x 300 x 200mm parcel fits Medium (listed as 300 x 400 x 200mm).
3. **Exactly 25kg is accepted.** The brief rejects parcels *over* 25kg, so only weights strictly greater than 25kg get no solution.
4. **Weight only acts as a cutoff.** It does not change which box is chosen. The box is picked from the dimensions alone, and the cheapest box that fits wins.
5. **There is no minimum size.** Any parcel with positive dimensions fits Small at the least. A label-size minimum is not in the brief, so I have not invented one.
6. **Invalid input is an error, not "no solution".** Zero, negative or non-numeric dimensions or weight are rejected as invalid. "No solution" is kept for a valid parcel that is too big or too heavy.
7. **Units and precision.** Dimensions are in millimetres and weight is in kilograms. Decimals are allowed.
8. **Entry point and data.** See the design decisions below.

## Design decisions

1. **Layers.** The box-choosing logic lives in a core library with no knowledge of how it is called. A thin Web API (controllers, no views) sits on top. If time is short, the core library and its tests come first and the API follows.
2. **DTOs stay at the edge.** Request and response DTOs belong to the API layer. The core library uses its own `Parcel` type.
3. **Testing.** Unit tests cover the core logic: each box type, oversized, overweight, exact-boundary dimensions, exactly 25kg, rotation, and invalid input. The controller can be unit tested by calling it directly. One `WebApplicationFactory` test to prove the wiring is optional; broader integration tests are out of scope.
4. **Box catalogue is data.** Sizes and prices sit behind an interface so they can change without touching the logic. The first version is a list in code. Moving to `appsettings.json` (with reload on change) or a database should only change one class. That avoids building infrastructure the brief does not ask for.
5. **"No solution" is `null`.** A valid parcel that is too big or too heavy returns a null result, not an exception, because it is a normal outcome. `PackagingResult` therefore always holds a real box and a real cost. I weighed a `NotFound` enum value (easy to serialise, but allows nonsense such as "NotFound, $5.00") and a `TryX` method (familiar, but clumsy from a controller). The trade-off with `null` is that callers must check for it, so nullable reference types stay enabled and the API maps null to a clear response.
6. **Fit check compares sorted sides.** A volume check is not enough. A 300 x 300 x 300mm parcel has less volume than Large but is still too tall for it. Instead, sort the parcel's three sides and each box's three sides, then compare them pair by pair. Taking the cheapest box that fits gives the answer. This also handles rotation. *(Changed my mind from an initial volume-based idea after finding this counterexample.)*

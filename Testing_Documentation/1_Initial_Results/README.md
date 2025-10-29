# Initial Test Results - Before Fixes

**Date:** 29/10/2025
**Total Tests:** 118
**Passed:** 97 (82.2%)
**Failed:** 21 (17.8%)

## Test Summary
- ✅ DonationServiceTests: Mostly passing
- ✅ Most controller tests: Passing
- ❌ DonorsControllerTests: 8 failures (Moq issue)
- ❌ HomeControllerTests: 1 failure (HttpContext)
- ❌ AdminController tests: Multiple failures (TempData)

## Coverage Files
- `coverage.cobertura.xml` - Raw coverage data
- `terminal_output.png` - Test execution screenshot
- `CoverageReport/index.html` - Detailed HTML report

## Next Steps
See folder `2_Issues_And_Fixes` for resolution plan.
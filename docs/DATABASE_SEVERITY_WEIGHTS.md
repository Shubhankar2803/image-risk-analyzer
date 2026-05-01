# Database-Driven Severity Weights - Professional Implementation

## Overview

This implementation uses **database-stored severity weights** instead of hardcoding values. This is the professional approach that allows you to:

✅ **Adjust risk strictness without recompiling**  
✅ **Change thresholds dynamically**  
✅ **Fine-tune per category independently**  
✅ **Enable/disable categories without code changes**  

## Architecture

### Components

```
ML Model Prediction (Confidence)
         ↓
ImageAnalysisService (receives category + confidence from ML)
         ↓
RiskScoringService (calculates risk using database weights)
         ↓
RiskCategoryRepository (fetches severity from database)
         ↓
ImageAnalysis entity (stores complete analysis with risk metrics)
```

### Database Schema

```sql
CREATE TABLE RiskCategories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,    -- e.g., "Weapons"
    SeverityWeight INT NOT NULL,                    -- 0-10 scale
    ActionThreshold FLOAT NOT NULL,                 -- Confidence threshold
    RecommendedAction NVARCHAR(50) NOT NULL,       -- Auto-Block, Review, Allow
    Description NVARCHAR(500) NOT NULL,
    IsEnabled BIT NOT NULL DEFAULT 1,
    UpdatedAt DATETIME NOT NULL
);
```

## Risk Scoring Formula

$$\text{Final Risk Score} = \text{Confidence (0-1)} \times \text{Severity Weight (0-10)} \times 10 = \text{Score (0-100)}$$

### Example

| Component | Value | Calculation |
|-----------|-------|-------------|
| **ML Confidence** | 0.92 (92% sure) | - |
| **Severity Weight** | 7 (Weapons) | From database |
| **Multiplier** | 10 | Standard scale |
| **Final Risk Score** | **64.4** | 0.92 × 7 × 10 |
| **Action** | Review | 5.0-8.0 range |

## Action Thresholds

```
Score > 8.0     → Auto-Block (Instant deletion)
Score 5.0-8.0   → Review (Admin review queue)
Score < 5.0     → Allow (Upload permitted)
```

## Risk Categories (Initial Data)

| Category | Weight | Threshold | Action | Reason |
|----------|--------|-----------|--------|--------|
| Safe | 0 | 1.0 | No Action | Control group |
| Explicit_Porn | 10 | 0.6 | Auto-Block | Legal/policy risk |
| Violence_gore | 10 | 0.7 | Auto-Block | Harm policies |
| Hate_Symbols | 10 | 0.65 | Auto-Block | De-platforming risk |
| Softporn | 9 | 0.7 | Review | Suggestive content |
| Weapons | 7 | 0.75 | Review | Context-dependent |
| Hentai | 6 | 0.8 | Review | Platform-dependent |
| Sensitive_Documents | 6 | 0.5 | Review | Contains PII |
| VIolence_gore | 4 | 0.85 | Warning | Non-graphic violence |

## Changing Severity Weights

### Option 1: Direct Database Update

```sql
-- Make Weapons more strict (weight 7 → 9)
UPDATE RiskCategories
SET SeverityWeight = 9,
    UpdatedAt = GETUTCDATE()
WHERE CategoryName = 'Weapons';

-- The change takes effect immediately (cache expires in 1 hour)
```

### Option 2: API Endpoint (Future Enhancement)

```csharp
[HttpPut("api/admin/risk-categories/{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UpdateRiskCategory(int id, UpdateRiskCategoryRequest request)
{
    var updated = await _riskCategoryService.UpdateAsync(id, request);
    return Ok(updated);
}
```

## Caching Strategy

The `RiskCategoryRepository` implements **1-hour caching** for performance:

```csharp
// First call: Fetch from database
var category = await _repository.GetByCategoryNameAsync("Weapons");

// Next calls (within 1 hour): Return from cache
var category = await _repository.GetByCategoryNameAsync("Weapons");

// After 1 hour or update: Database refresh
```

### Cache Invalidation

Caching is automatically invalidated when:
- ✅ `CreateAsync()` is called
- ✅ `UpdateAsync()` is called
- ✅ `DeleteAsync()` is called

## Service Layer Architecture

### RiskScoringService

Handles all risk calculation logic:

```csharp
// Calculate risk with database weights
var result = await _riskScoringService.CalculateRiskAsync(
    category: "Weapons",
    confidence: 0.92f
);

// Returns:
// {
//   FinalRiskScore: 64.4,
//   Action: "Review",
//   SeverityWeight: 7,
//   RiskColor: "yellow"
// }
```

### RiskCategoryRepository

Provides data access with caching:

```csharp
// Fetch specific category
var category = await _repository.GetByCategoryNameAsync("Weapons");

// Update severity weight
category.SeverityWeight = 9;
await _repository.UpdateAsync(category);
```

## Implementation Flow

```
1. User uploads image
   ↓
2. ImageAnalysisService receives request
   ↓
3. MLModelService classifies image
   Returns: Category="Weapons", Confidence=0.92
   ↓
4. RiskScoringService calculates risk
   Fetches: SeverityWeight=7 from database
   Calculates: 0.92 × 7 × 10 = 64.4
   ↓
5. ImageAnalysisService saves analysis
   Stores all metrics: RiskScore=64.4, Action="Review", Color="yellow"
   ↓
6. Frontend displays with color-coded UI
   Risk Score 64.4 → Yellow (Warning)
```

## Database Migration

The migration automatically:
- ✅ Creates `RiskCategories` table
- ✅ Seeds initial category data
- ✅ Creates indexes for performance

Run migration:
```bash
cd backend/src/RiskAnalyzer.Api
dotnet ef database update
```

## Adjusting Thresholds in Production

### Scenario: Make weapons detection more strict

```sql
-- Increase weight from 7 to 9
UPDATE RiskCategories
SET SeverityWeight = 9
WHERE CategoryName = 'Weapons';

-- Impact:
-- Confidence 0.92 now gives: 0.92 × 9 × 10 = 82.8 (was 64.4)
-- Action changes: Review → Auto-Block
```

### Scenario: Reduce false positives on violence

```sql
-- Lower threshold from 0.7 to 0.8 (higher = stricter needed)
UPDATE RiskCategories
SET ActionThreshold = 0.8
WHERE CategoryName = 'Violence_gore';

-- Now requires higher confidence before auto-blocking
```

## Monitoring & Logging

All calculations are logged for audit trail:

```
Risk calculated: Category=Weapons, 
Confidence=92.00%, 
SeverityWeight=7, 
FinalScore=64.40, 
Action=Review
```

## Best Practices

1. **Start Conservative**: Keep high severity weights initially
2. **Monitor False Positives**: Adjust thresholds based on real-world data
3. **Regular Reviews**: Check which categories trigger most reviews
4. **Test Changes**: Update one category and monitor impact
5. **Document Changes**: Log why you changed weights
6. **Audit Trail**: Keep track of all weight adjustments

## Error Handling

If a category is not found in database:
- ✅ Returns "Safe" with weight=0
- ✅ Logs warning for investigation
- ✅ Allows upload but flags for admin review

## Security

- ✅ Only admin can modify weights (future: add role-based access)
- ✅ Changes logged in database
- ✅ No hardcoded defaults (always database-driven)
- ✅ Cache respects database state

## Next Steps

1. **Run migration** to create RiskCategories table
2. **Populate training data** with images per category
3. **Train ML model** using these categories
4. **Test predictions** and monitor accuracy
5. **Adjust weights** based on real-world results
6. **Implement admin dashboard** for managing categories

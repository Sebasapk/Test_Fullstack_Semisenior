-- Example queries: tasks by user and JSON functions over Tasks.AdditionalData. Leaves the data unchanged; safe to re-run.
USE TaskManagerDb;
GO

-- Tasks of one user, optionally filtered by status (NULL = all statuses), newest first.
DECLARE @UserId INT = 1;
DECLARE @Status NVARCHAR(20) = NULL; -- 'Pending', 'InProgress' or 'Done'

SELECT t.Id, t.Title, t.Description, t.Status, t.UserId, u.Name AS UserName, t.CreatedAt, t.UpdatedAt, t.AdditionalData
FROM dbo.Tasks AS t
INNER JOIN dbo.Users AS u ON u.Id = t.UserId
WHERE t.UserId = @UserId
  AND (@Status IS NULL OR t.Status = @Status)
ORDER BY t.CreatedAt DESC;
GO

-- ISJSON: validates text. The CHECK constraint on AdditionalData uses it to reject malformed JSON.
SELECT Id, Title, ISJSON(AdditionalData) AS IsValidJson
FROM dbo.Tasks;
GO

-- JSON_VALUE: reads a scalar (priority) and filters by it.
SELECT Id, Title, JSON_VALUE(AdditionalData, '$.priority') AS Priority
FROM dbo.Tasks
WHERE JSON_VALUE(AdditionalData, '$.priority') = 'High';
GO

-- JSON_QUERY: returns an array or object as JSON text (JSON_VALUE would return NULL for these).
SELECT Id, Title, JSON_QUERY(AdditionalData, '$.tags') AS Tags
FROM dbo.Tasks
WHERE AdditionalData IS NOT NULL;
GO

-- OPENJSON + CROSS APPLY: expands the tags array into rows to filter tasks by a single tag.
SELECT t.Id, t.Title, tag.value AS Tag
FROM dbo.Tasks AS t
CROSS APPLY OPENJSON(t.AdditionalData, '$.tags') AS tag
WHERE tag.value = 'backend';
GO

-- JSON_MODIFY: changes the priority of one task. Rolled back so the seed data stays intact.
BEGIN TRANSACTION;

UPDATE dbo.Tasks
SET AdditionalData = JSON_MODIFY(AdditionalData, '$.priority', 'Low'),
    UpdatedAt = SYSUTCDATETIME()
WHERE Id = (SELECT TOP (1) Id FROM dbo.Tasks WHERE JSON_VALUE(AdditionalData, '$.priority') = 'High' ORDER BY Id);

SELECT Id, Title, JSON_VALUE(AdditionalData, '$.priority') AS PriorityAfterUpdate
FROM dbo.Tasks
WHERE Id = (SELECT TOP (1) Id FROM dbo.Tasks WHERE JSON_VALUE(AdditionalData, '$.priority') = 'Low' ORDER BY UpdatedAt DESC);

ROLLBACK TRANSACTION;
GO

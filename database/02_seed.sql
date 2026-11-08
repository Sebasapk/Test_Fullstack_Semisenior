-- Optional seed with sample users and tasks for testing. Safe to re-run: existing rows are replaced.
USE TaskManagerDb;
GO

DELETE FROM dbo.Tasks;
DELETE FROM dbo.Users;
GO

INSERT INTO dbo.Users (Name, Email) VALUES
    (N'Ana Torres',   N'ana.torres@example.com'),
    (N'Carlos Ruiz',  N'carlos.ruiz@example.com'),
    (N'Laura Gomez',  N'laura.gomez@example.com');

-- Ids are looked up by email because identity values change on every re-run.
DECLARE @Ana    INT = (SELECT Id FROM dbo.Users WHERE Email = N'ana.torres@example.com');
DECLARE @Carlos INT = (SELECT Id FROM dbo.Users WHERE Email = N'carlos.ruiz@example.com');
DECLARE @Laura  INT = (SELECT Id FROM dbo.Users WHERE Email = N'laura.gomez@example.com');

-- Explicit CreatedAt values spread over several days so ordering and filtering are easy to check.
INSERT INTO dbo.Tasks (Title, Description, Status, UserId, CreatedAt, UpdatedAt, AdditionalData) VALUES
    (N'Design database schema',  N'Define tables, keys and constraints.',      'Done',       @Ana,    '2026-11-01T09:00:00', '2026-11-02T17:00:00',
        N'{"priority":"High","estimatedEndDate":"2026-11-03","tags":["database","design"],"metadata":{"source":"web"}}'),
    (N'Build users endpoints',   N'Create and list users.',                    'Done',       @Ana,    '2026-11-02T10:00:00', '2026-11-04T12:00:00',
        N'{"priority":"Medium","estimatedEndDate":"2026-11-05","tags":["backend","api"],"metadata":{"source":"web"}}'),
    (N'Build tasks endpoints',   N'Create, list and update task status.',      'InProgress', @Ana,    '2026-11-04T08:30:00', '2026-11-06T11:00:00',
        N'{"priority":"High","estimatedEndDate":"2026-11-20","tags":["backend","api"],"metadata":{"source":"web"}}'),
    (N'Write unit tests',        N'Cover task status transitions.',            'Pending',    @Ana,    '2026-11-05T14:00:00', '2026-11-05T14:00:00',
        N'{"priority":"Low","estimatedEndDate":"2026-11-25","tags":["backend","testing"],"metadata":{"source":"mobile"}}'),
    (N'Create Angular project',  N'Scaffold the frontend application.',        'Done',       @Carlos, '2026-11-03T09:15:00', '2026-11-04T16:00:00',
        N'{"priority":"Medium","estimatedEndDate":"2026-11-05","tags":["frontend"],"metadata":{"source":"web"}}'),
    (N'Task list screen',        N'Show tasks with a status filter.',          'InProgress', @Carlos, '2026-11-06T10:00:00', '2026-11-07T09:30:00',
        N'{"priority":"High","estimatedEndDate":"2026-11-18","tags":["frontend","ui"],"metadata":{"source":"web"}}'),
    (N'Reactive task form',      N'Form with validation and JSON fields.',     'Pending',    @Carlos, '2026-11-07T11:00:00', '2026-11-07T11:00:00',
        N'{"priority":"Medium","estimatedEndDate":"2026-11-22","tags":["frontend","forms"],"metadata":{"source":"web"}}'),
    (N'Write README',            N'Document setup and technical decisions.',   'Pending',    @Laura,  '2026-11-07T15:00:00', '2026-11-07T15:00:00',
        N'{"priority":"Low","estimatedEndDate":"2026-11-28","tags":["docs"],"metadata":{"source":"web"}}'),
    (N'Review requirements',     N'Check the work against the specification.', 'InProgress', @Laura,  '2026-11-08T08:00:00', '2026-11-08T10:00:00',
        NULL);
GO

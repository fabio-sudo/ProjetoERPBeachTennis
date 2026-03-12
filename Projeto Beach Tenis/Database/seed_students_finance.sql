USE ArenaManagementDB;
GO

-- 1. Inserir Planos (Caso não existam)
IF NOT EXISTS (SELECT * FROM Plans WHERE Name = 'Basic')
    INSERT INTO Plans (Name, Description, Price, DurationDays, IsActive, CreatedAt) VALUES ('Basic', 'Acesso de 2x na semana', 150.00, 30, 1, GETDATE());

IF NOT EXISTS (SELECT * FROM Plans WHERE Name = 'Pro')
    INSERT INTO Plans (Name, Description, Price, DurationDays, IsActive, CreatedAt) VALUES ('Pro', 'Acesso Livre', 250.00, 30, 1, GETDATE());

IF NOT EXISTS (SELECT * FROM Plans WHERE Name = 'VIP')
    INSERT INTO Plans (Name, Description, Price, DurationDays, IsActive, CreatedAt) VALUES ('VIP', 'Acesso Livre + Aulas', 400.00, 30, 1, GETDATE());

-- Vamos pegar os IDs dos planos
DECLARE @BasicPlanId INT = (SELECT TOP 1 Id FROM Plans WHERE Name = 'Basic');
DECLARE @ProPlanId INT = (SELECT TOP 1 Id FROM Plans WHERE Name = 'Pro');
DECLARE @VIPPlanId INT = (SELECT TOP 1 Id FROM Plans WHERE Name = 'VIP');


-- 2. Inserir Alunos de Simulação
-- Aluno 1: Tudo Pago
INSERT INTO Students (Name, Phone, Email, PlanName, CreatedAt, StartDate) 
VALUES ('Carlos Eduardo (Em Dia)', '11999991111', 'carlos.emdia@teste.com', 'Pro', GETDATE(), DATEADD(MONTH, -2, GETDATE()));
DECLARE @Student1Id INT = SCOPE_IDENTITY();

-- Aluno 2: Pendente (Vence Hoje/Amanhã)
INSERT INTO Students (Name, Phone, Email, PlanName, CreatedAt, StartDate) 
VALUES ('Mariana Silva (Pendente)', '11999992222', 'mariana.pendente@teste.com', 'Basic', GETDATE(), DATEADD(MONTH, -1, GETDATE()));
DECLARE @Student2Id INT = SCOPE_IDENTITY();

-- Aluno 3: Atrasado (Inadimplente)
INSERT INTO Students (Name, Phone, Email, PlanName, CreatedAt, StartDate) 
VALUES ('Rafael Souza (Atrasado)', '11999993333', 'rafael.atrasado@teste.com', 'VIP', GETDATE(), DATEADD(MONTH, -3, GETDATE()));
DECLARE @Student3Id INT = SCOPE_IDENTITY();


-- 3. Inserir Assinaturas (StudentSubscriptions)
-- Assinatura Carlos
INSERT INTO StudentSubscriptions (StudentId, PlanId, StartDate, NextBillingDate, Status, AutoRenew, CreatedAt)
VALUES (@Student1Id, @ProPlanId, DATEADD(MONTH, -2, GETDATE()), DATEADD(MONTH, 1, GETDATE()), 'Active', 1, GETDATE());
DECLARE @Sub1Id INT = SCOPE_IDENTITY();

-- Assinatura Mariana
INSERT INTO StudentSubscriptions (StudentId, PlanId, StartDate, NextBillingDate, Status, AutoRenew, CreatedAt)
VALUES (@Student2Id, @BasicPlanId, DATEADD(MONTH, -1, GETDATE()), GETDATE(), 'Active', 1, GETDATE());
DECLARE @Sub2Id INT = SCOPE_IDENTITY();

-- Assinatura Rafael
INSERT INTO StudentSubscriptions (StudentId, PlanId, StartDate, NextBillingDate, Status, AutoRenew, CreatedAt)
VALUES (@Student3Id, @VIPPlanId, DATEADD(MONTH, -3, GETDATE()), DATEADD(MONTH, -1, GETDATE()), 'Active', 1, GETDATE());
DECLARE @Sub3Id INT = SCOPE_IDENTITY();


-- 4. Inserir Mensalidades (StudentPayments)

-- Carlos: Pagou as duas últimas
INSERT INTO StudentPayments (StudentSubscriptionId, Amount, DueDate, PaymentDate, Status, PaymentMethod, CreatedAt)
VALUES (@Sub1Id, 250.00, DATEADD(MONTH, -2, GETDATE()), DATEADD(MONTH, -2, GETDATE()), 'Paid', 'PIX', GETDATE());

INSERT INTO StudentPayments (StudentSubscriptionId, Amount, DueDate, PaymentDate, Status, PaymentMethod, CreatedAt)
VALUES (@Sub1Id, 250.00, DATEADD(MONTH, -1, GETDATE()), DATEADD(MONTH, -1, GETDATE()), 'Paid', 'Cartão de Crédito', GETDATE());

-- Mariana: Pagou a primeira, a segunda está pendente para hoje
INSERT INTO StudentPayments (StudentSubscriptionId, Amount, DueDate, PaymentDate, Status, PaymentMethod, CreatedAt)
VALUES (@Sub2Id, 150.00, DATEADD(MONTH, -1, GETDATE()), DATEADD(MONTH, -1, GETDATE()), 'Paid', 'Dinheiro', GETDATE());

INSERT INTO StudentPayments (StudentSubscriptionId, Amount, DueDate, Status, PaymentMethod, CreatedAt)
VALUES (@Sub2Id, 150.00, GETDATE(), 'Pending', '-', GETDATE());

-- Rafael: Pagou a primeira, e tem duas atrasadas
INSERT INTO StudentPayments (StudentSubscriptionId, Amount, DueDate, PaymentDate, Status, PaymentMethod, CreatedAt)
VALUES (@Sub3Id, 400.00, DATEADD(MONTH, -3, GETDATE()), DATEADD(MONTH, -3, GETDATE()), 'Paid', 'PIX', GETDATE());

INSERT INTO StudentPayments (StudentSubscriptionId, Amount, DueDate, Status, PaymentMethod, CreatedAt)
VALUES (@Sub3Id, 400.00, DATEADD(MONTH, -2, GETDATE()), 'Overdue', '-', GETDATE());

INSERT INTO StudentPayments (StudentSubscriptionId, Amount, DueDate, Status, PaymentMethod, CreatedAt)
VALUES (@Sub3Id, 400.00, DATEADD(MONTH, -1, GETDATE()), 'Overdue', '-', GETDATE());

PRINT 'Dados simulados de Alunos e Mensalidades inseridos com sucesso!';
GO

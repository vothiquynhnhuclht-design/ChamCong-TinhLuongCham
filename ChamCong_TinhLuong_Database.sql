-- ======================================================================================
-- HỆ THỐNG QUẢN TRỊ NHÂN SỰ & CHẤM CÔNG - TÍNH LƯƠNG TỰ ĐỘNG (HRM & PAYROLL)
-- KỊCH BẢN KHỞI TẠO CƠ SỞ DỮ LIỆU SQL SERVER TOÀN DIỆN (DDL & DML)
-- Tác giả: Nhóm Đồ Án Capstone HRM & Payroll (Võ Thị Quỳnh Như)
-- Đảm bảo tương thích 100% Entity Framework Core và Kiến trúc Clean Architecture
-- ======================================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'ChamCong_TinhLuong_DB')
BEGIN
    CREATE DATABASE [ChamCong_TinhLuong_DB];
END
GO

USE [ChamCong_TinhLuong_DB];
GO

-- ======================================================================================
-- PHẦN 1: TẠO CẤU TRÚC BẢNG (DDL)
-- ======================================================================================

-- 1. Bảng Phòng Ban (Departments)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Departments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Departments] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DepartmentCode] NVARCHAR(50) NOT NULL UNIQUE,
        [DepartmentName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(255) NULL
    );
END
GO

-- 2. Bảng Ca Làm Việc (WorkShifts)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkShifts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WorkShifts] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ShiftCode] NVARCHAR(50) NOT NULL UNIQUE,
        [ShiftName] NVARCHAR(100) NOT NULL,
        [StartTime] TIME(7) NOT NULL,
        [EndTime] TIME(7) NOT NULL,
        [BreakStartTime] TIME(7) NULL,
        [BreakEndTime] TIME(7) NULL,
        [WorkDayFactor] FLOAT NOT NULL DEFAULT 1.0
    );
END
GO

-- 3. Bảng Danh Mục Trích Nộp & Thuế (DeductionRates)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeductionRates]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DeductionRates] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Code] NVARCHAR(50) NOT NULL UNIQUE,
        [Name] NVARCHAR(150) NOT NULL,
        [RatePercentage] DECIMAL(6,4) NULL,
        [FlatAmount] DECIMAL(18,2) NULL,
        [ApplyOn] NVARCHAR(50) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [RegulationSource] NVARCHAR(255) NULL
    );
END
GO

-- 4. Bảng Nhân Viên (Employees)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Employees] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeCode] NVARCHAR(20) NOT NULL UNIQUE,
        [FullName] NVARCHAR(150) NOT NULL,
        [Email] NVARCHAR(150) NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [Position] NVARCHAR(100) NULL,
        [JoinDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [DependentCount] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [DepartmentId] INT NOT NULL,
        CONSTRAINT [FK_Employees_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments] ([Id]) ON DELETE NO ACTION
    );
END
GO

-- 5. Bảng Hợp Đồng Lao Động (Contracts)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contracts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Contracts] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeId] INT NOT NULL,
        [ContractNumber] NVARCHAR(50) NOT NULL,
        [ContractType] INT NOT NULL,
        [BasicSalary] DECIMAL(18,2) NOT NULL,
        [Allowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [InsuranceSalary] DECIMAL(18,2) NOT NULL,
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NULL,
        [IsCurrent] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_Contracts_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE CASCADE
    );
END
GO

-- 6. Bảng Kỳ Lương (PayrollPeriods)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PayrollPeriods]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PayrollPeriods] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Month] INT NOT NULL,
        [Year] INT NOT NULL,
        [StandardWorkDays] DECIMAL(4,1) NOT NULL DEFAULT 22.0,
        [Status] INT NOT NULL DEFAULT 0,
        [LockedAt] DATETIME2 NULL,
        [LockedBy] NVARCHAR(100) NULL,
        [Note] NVARCHAR(255) NULL,
        CONSTRAINT [UQ_PayrollPeriod_Month_Year] UNIQUE ([Month], [Year])
    );
END
GO

-- 7. Bảng Tài Khoản Người Dùng (AppUsers)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AppUsers] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(256) NOT NULL,
        [Role] INT NOT NULL DEFAULT 2, -- 0: Admin, 1: HR, 2: Employee
        [EmployeeId] INT NULL UNIQUE,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [LastLoginAt] DATETIME2 NULL,
        CONSTRAINT [FK_AppUsers_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE SET NULL
    );
END
GO

-- 8. Bảng Chấm Công (TimeAttendances)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TimeAttendances]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TimeAttendances] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeId] INT NOT NULL,
        [WorkDate] DATETIME2 NOT NULL,
        [CheckIn] TIME(7) NULL,
        [CheckOut] TIME(7) NULL,
        [ActualWorkDays] FLOAT NOT NULL DEFAULT 0,
        [OvertimeHours] FLOAT NOT NULL DEFAULT 0,
        [LateMinutes] INT NOT NULL DEFAULT 0,
        [EarlyLeaveMinutes] INT NOT NULL DEFAULT 0,
        [Status] INT NOT NULL DEFAULT 0,
        [WorkShiftId] INT NULL,
        [PhotoPath] NVARCHAR(500) NULL,
        [CheckOutPhotoPath] NVARCHAR(500) NULL,
        [Note] NVARCHAR(255) NULL,
        CONSTRAINT [FK_TimeAttendances_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TimeAttendances_WorkShifts] FOREIGN KEY ([WorkShiftId]) REFERENCES [dbo].[WorkShifts] ([Id]) ON DELETE SET NULL
    );
END
GO

-- 9. Bảng Phiếu Lương (Payslips - Header)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payslips]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Payslips] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeId] INT NOT NULL,
        [PayrollPeriodId] INT NULL,
        [Month] INT NOT NULL,
        [Year] INT NOT NULL,
        [StandardWorkDays] FLOAT NOT NULL DEFAULT 22,
        [ActualWorkDays] FLOAT NOT NULL DEFAULT 0,
        [OvertimeHours] FLOAT NOT NULL DEFAULT 0,
        [BasicSalary] DECIMAL(18,2) NOT NULL,
        [Allowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [GrossSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalDeductions] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [NetSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Status] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [LockedAt] DATETIME2 NULL,
        [LockedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_Payslips_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Payslips_PayrollPeriods] FOREIGN KEY ([PayrollPeriodId]) REFERENCES [dbo].[PayrollPeriods] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [UQ_Payslips_Employee_Month_Year] UNIQUE ([EmployeeId], [Month], [Year])
    );
END
GO

-- 10. Bảng Chi Tiết Phiếu Lương (PayslipLines - Detail Line)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PayslipLines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PayslipLines] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PayslipId] INT NOT NULL,
        [LineType] INT NOT NULL, -- 0: Earning, 1: Deduction
        [ItemCode] NVARCHAR(50) NOT NULL,
        [ItemName] NVARCHAR(150) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [RateApplied] DECIMAL(6,4) NULL,
        [CalculationNote] NVARCHAR(255) NULL,
        [DeductionRateId] INT NULL,
        CONSTRAINT [FK_PayslipLines_Payslips] FOREIGN KEY ([PayslipId]) REFERENCES [dbo].[Payslips] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PayslipLines_DeductionRates] FOREIGN KEY ([DeductionRateId]) REFERENCES [dbo].[DeductionRates] ([Id]) ON DELETE SET NULL
    );
END
GO

-- 11. Bảng Đơn Nghỉ Phép (LeaveRequests)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LeaveRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LeaveRequests] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeId] INT NOT NULL,
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NOT NULL,
        [TotalDays] FLOAT NOT NULL,
        [LeaveType] INT NOT NULL DEFAULT 0,
        [Reason] NVARCHAR(500) NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [ApprovedBy] NVARCHAR(100) NULL,
        [ApprovedAt] DATETIME2 NULL,
        [Note] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_LeaveRequests_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE CASCADE
    );
END
GO

-- 12. Bảng Lịch Sử Thông Báo & Gửi Phiếu Lương (NotificationLogs)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NotificationLogs] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeId] INT NOT NULL,
        [PayslipId] INT NULL,
        [RecipientEmail] NVARCHAR(150) NOT NULL,
        [Subject] NVARCHAR(255) NOT NULL,
        [Type] INT NOT NULL DEFAULT 0,
        [IsSuccess] BIT NOT NULL DEFAULT 1,
        [ErrorMessage] NVARCHAR(1000) NULL,
        [SentAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_NotificationLogs_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_NotificationLogs_Payslips] FOREIGN KEY ([PayslipId]) REFERENCES [dbo].[Payslips] ([Id]) ON DELETE SET NULL
    );
END
GO

-- ======================================================================================
-- PHẦN 2: ĐỒNG BỘ DỮ LIỆU CHUẨN DOANH NGHIỆP VÀO SQL SERVER (DML)
-- ======================================================================================

-- 1. Nạp danh mục Phòng Ban
IF NOT EXISTS (SELECT 1 FROM [Departments] WHERE [DepartmentCode] = 'IT')
    INSERT INTO [Departments] ([DepartmentCode], [DepartmentName], [Description])
    VALUES (N'IT', N'Phòng Công Nghệ Thông Tin', N'Phát triển, bảo trì và vận hành hệ thống phần mềm doanh nghiệp');

IF NOT EXISTS (SELECT 1 FROM [Departments] WHERE [DepartmentCode] = 'HR')
    INSERT INTO [Departments] ([DepartmentCode], [DepartmentName], [Description])
    VALUES (N'HR', N'Phòng Nhân Sự - Kế Toán', N'Quản trị nguồn nhân lực, tính công và chi trả lương');

IF NOT EXISTS (SELECT 1 FROM [Departments] WHERE [DepartmentCode] = 'MKT')
    INSERT INTO [Departments] ([DepartmentCode], [DepartmentName], [Description])
    VALUES (N'MKT', N'Phòng Kinh Doanh & Marketing', N'Mở rộng thị trường, tư vấn giải pháp và quan hệ khách hàng');

-- 2. Nạp danh mục Ca Làm Việc
IF NOT EXISTS (SELECT 1 FROM [WorkShifts] WHERE [ShiftCode] = 'HC')
    INSERT INTO [WorkShifts] ([ShiftCode], [ShiftName], [StartTime], [EndTime], [BreakStartTime], [BreakEndTime], [WorkDayFactor])
    VALUES ('HC', N'Ca Hành Chính', '08:00:00', '17:30:00', '12:00:00', '13:30:00', 1.0);

IF NOT EXISTS (SELECT 1 FROM [WorkShifts] WHERE [ShiftCode] = 'CA1')
    INSERT INTO [WorkShifts] ([ShiftCode], [ShiftName], [StartTime], [EndTime], [BreakStartTime], [BreakEndTime], [WorkDayFactor])
    VALUES ('CA1', N'Ca Sáng', '06:00:00', '14:00:00', '11:00:00', '12:00:00', 1.0);

IF NOT EXISTS (SELECT 1 FROM [WorkShifts] WHERE [ShiftCode] = 'CA2')
    INSERT INTO [WorkShifts] ([ShiftCode], [ShiftName], [StartTime], [EndTime], [BreakStartTime], [BreakEndTime], [WorkDayFactor])
    VALUES ('CA2', N'Ca Chiều', '14:00:00', '22:00:00', '18:00:00', '19:00:00', 1.0);

-- 3. Nạp danh mục Tỷ Lệ Trích Nộp & Giảm Trừ Thuế theo Luật Việt Nam
IF NOT EXISTS (SELECT 1 FROM [DeductionRates] WHERE [Code] = 'BHXH')
    INSERT INTO [DeductionRates] ([Code], [Name], [RatePercentage], [FlatAmount], [ApplyOn], [IsActive], [RegulationSource])
    VALUES ('BHXH', N'Bảo hiểm xã hội (8%)', 0.0800, NULL, 'InsuranceSalary', 1, N'Luật BHXH số 58/2014/QH13, Điều 85');

IF NOT EXISTS (SELECT 1 FROM [DeductionRates] WHERE [Code] = 'BHYT')
    INSERT INTO [DeductionRates] ([Code], [Name], [RatePercentage], [FlatAmount], [ApplyOn], [IsActive], [RegulationSource])
    VALUES ('BHYT', N'Bảo hiểm y tế (1.5%)', 0.0150, NULL, 'InsuranceSalary', 1, N'Luật BHYT số 46/2014/QH13, Điều 13');

IF NOT EXISTS (SELECT 1 FROM [DeductionRates] WHERE [Code] = 'BHTN')
    INSERT INTO [DeductionRates] ([Code], [Name], [RatePercentage], [FlatAmount], [ApplyOn], [IsActive], [RegulationSource])
    VALUES ('BHTN', N'Bảo hiểm thất nghiệp (1%)', 0.0100, NULL, 'InsuranceSalary', 1, N'Luật Việc làm số 38/2013/QH13, Điều 57');

IF NOT EXISTS (SELECT 1 FROM [DeductionRates] WHERE [Code] = 'TNCN_BAN_THAN')
    INSERT INTO [DeductionRates] ([Code], [Name], [RatePercentage], [FlatAmount], [ApplyOn], [IsActive], [RegulationSource])
    VALUES ('TNCN_BAN_THAN', N'Giảm trừ gia cảnh bản thân', NULL, 11000000.00, 'TaxableIncome', 1, N'Nghị quyết 954/2020/UBTVQH14 (11 tr/tháng)');

IF NOT EXISTS (SELECT 1 FROM [DeductionRates] WHERE [Code] = 'TNCN_PHU_THUOC')
    INSERT INTO [DeductionRates] ([Code], [Name], [RatePercentage], [FlatAmount], [ApplyOn], [IsActive], [RegulationSource])
    VALUES ('TNCN_PHU_THUOC', N'Giảm trừ người phụ thuộc', NULL, 4400000.00, 'TaxableIncome', 1, N'Nghị quyết 954/2020/UBTVQH14 (4.4 tr/tháng)');

-- 4. Nạp Danh Sách Nhân Viên (Employees)
DECLARE @DeptIT INT = (SELECT TOP 1 [Id] FROM [Departments] WHERE [DepartmentCode] = 'IT');
DECLARE @DeptHR INT = (SELECT TOP 1 [Id] FROM [Departments] WHERE [DepartmentCode] = 'HR');
DECLARE @DeptMKT INT = (SELECT TOP 1 [Id] FROM [Departments] WHERE [DepartmentCode] = 'MKT');

IF NOT EXISTS (SELECT 1 FROM [Employees] WHERE [EmployeeCode] = 'NV001')
    INSERT INTO [Employees] ([EmployeeCode], [FullName], [Email], [PhoneNumber], [Position], [JoinDate], [DependentCount], [IsActive], [DepartmentId])
    VALUES ('NV001', N'Nguyễn Văn An', 'an.nguyen@company.com', '0901234567', N'Senior Fullstack Developer', '2023-01-15', 1, 1, @DeptIT);

IF NOT EXISTS (SELECT 1 FROM [Employees] WHERE [EmployeeCode] = 'NV002')
    INSERT INTO [Employees] ([EmployeeCode], [FullName], [Email], [PhoneNumber], [Position], [JoinDate], [DependentCount], [IsActive], [DepartmentId])
    VALUES ('NV002', N'Trần Thị Bình', 'binh.tran@company.com', '0912345678', N'HR Manager', '2022-05-10', 0, 1, @DeptHR);

IF NOT EXISTS (SELECT 1 FROM [Employees] WHERE [EmployeeCode] = 'NV003')
    INSERT INTO [Employees] ([EmployeeCode], [FullName], [Email], [PhoneNumber], [Position], [JoinDate], [DependentCount], [IsActive], [DepartmentId])
    VALUES ('NV003', N'Lê Hoàng Cường', 'cuong.le@company.com', '0923456789', N'Marketing Specialist', '2024-02-01', 0, 1, @DeptMKT);

IF NOT EXISTS (SELECT 1 FROM [Employees] WHERE [EmployeeCode] = 'NV004')
    INSERT INTO [Employees] ([EmployeeCode], [FullName], [Email], [PhoneNumber], [Position], [JoinDate], [DependentCount], [IsActive], [DepartmentId])
    VALUES ('NV004', N'Võ Thị Quỳnh Như', 'Vothiquynhnhuclht@gmail.com', '0934567890', N'Chuyên viên Tư vấn Giải pháp', '2022-01-01', 0, 1, @DeptIT);

IF NOT EXISTS (SELECT 1 FROM [Employees] WHERE [EmployeeCode] = 'NV005')
    INSERT INTO [Employees] ([EmployeeCode], [FullName], [Email], [PhoneNumber], [Position], [JoinDate], [DependentCount], [IsActive], [DepartmentId])
    VALUES ('NV005', N'Lê Linh', 'Linhle123@gmailcom', '0945678901', N'Business Analyst', '2023-06-15', 0, 1, @DeptIT);

-- 5. Nạp Hợp Đồng Lao Động (Contracts)
DECLARE @Emp1 INT = (SELECT TOP 1 [Id] FROM [Employees] WHERE [EmployeeCode] = 'NV001');
DECLARE @Emp2 INT = (SELECT TOP 1 [Id] FROM [Employees] WHERE [EmployeeCode] = 'NV002');
DECLARE @Emp3 INT = (SELECT TOP 1 [Id] FROM [Employees] WHERE [EmployeeCode] = 'NV003');
DECLARE @Emp4 INT = (SELECT TOP 1 [Id] FROM [Employees] WHERE [EmployeeCode] = 'NV004');
DECLARE @Emp5 INT = (SELECT TOP 1 [Id] FROM [Employees] WHERE [EmployeeCode] = 'NV005');

IF NOT EXISTS (SELECT 1 FROM [Contracts] WHERE [EmployeeId] = @Emp1)
    INSERT INTO [Contracts] ([EmployeeId], [ContractNumber], [ContractType], [BasicSalary], [Allowance], [InsuranceSalary], [StartDate], [IsCurrent])
    VALUES (@Emp1, 'HD-2023/001', 1, 20000000.00, 2500000.00, 20000000.00, '2023-01-15', 1);

IF NOT EXISTS (SELECT 1 FROM [Contracts] WHERE [EmployeeId] = @Emp2)
    INSERT INTO [Contracts] ([EmployeeId], [ContractNumber], [ContractType], [BasicSalary], [Allowance], [InsuranceSalary], [StartDate], [IsCurrent])
    VALUES (@Emp2, 'HD-2022/045', 1, 16000000.00, 2000000.00, 16000000.00, '2022-05-10', 1);

IF NOT EXISTS (SELECT 1 FROM [Contracts] WHERE [EmployeeId] = @Emp3)
    INSERT INTO [Contracts] ([EmployeeId], [ContractNumber], [ContractType], [BasicSalary], [Allowance], [InsuranceSalary], [StartDate], [IsCurrent])
    VALUES (@Emp3, 'HD-2024/012', 1, 12000000.00, 1500000.00, 12000000.00, '2024-02-01', 1);

IF NOT EXISTS (SELECT 1 FROM [Contracts] WHERE [EmployeeId] = @Emp4)
    INSERT INTO [Contracts] ([EmployeeId], [ContractNumber], [ContractType], [BasicSalary], [Allowance], [InsuranceSalary], [StartDate], [IsCurrent])
    VALUES (@Emp4, 'HD-2022/088', 1, 22000000.00, 3000000.00, 22000000.00, '2022-01-01', 1);

IF NOT EXISTS (SELECT 1 FROM [Contracts] WHERE [EmployeeId] = @Emp5)
    INSERT INTO [Contracts] ([EmployeeId], [ContractNumber], [ContractType], [BasicSalary], [Allowance], [InsuranceSalary], [StartDate], [IsCurrent])
    VALUES (@Emp5, 'HD-2023/099', 1, 15000000.00, 2000000.00, 15000000.00, '2023-06-15', 1);

-- 6. Nạp Kỳ Lương (PayrollPeriods)
IF NOT EXISTS (SELECT 1 FROM [PayrollPeriods] WHERE [Month] = 9 AND [Year] = 2026)
    INSERT INTO [PayrollPeriods] ([Month], [Year], [StandardWorkDays], [Status], [LockedAt], [LockedBy], [Note])
    VALUES (9, 2026, 22.0, 1, '2026-10-05 17:00:00', N'ketoan@company.com', N'Kỳ lương Tháng 09/2026 đã chốt khóa sổ');

IF NOT EXISTS (SELECT 1 FROM [PayrollPeriods] WHERE [Month] = 10 AND [Year] = 2026)
    INSERT INTO [PayrollPeriods] ([Month], [Year], [StandardWorkDays], [Status], [LockedAt], [LockedBy], [Note])
    VALUES (10, 2026, 22.0, 0, NULL, NULL, N'Kỳ lương Tháng 10/2026 đang xử lý');

-- 7. Nạp Tài Khoản Đăng Nhập Hệ Thống (AppUsers)
-- Mật khẩu mặc định SHA-256 của "123456": 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
DECLARE @PwdHash NVARCHAR(256) = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92';

-- Tài khoản Quản trị toàn quyền
IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Username] = 'admin')
    INSERT INTO [AppUsers] ([Username], [PasswordHash], [Role], [EmployeeId], [IsActive], [CreatedAt])
    VALUES ('admin', @PwdHash, 0, NULL, 1, GETDATE());

-- Tài khoản Kế toán trưởng
IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Username] = 'ketoan')
    INSERT INTO [AppUsers] ([Username], [PasswordHash], [Role], [EmployeeId], [IsActive], [CreatedAt])
    VALUES ('ketoan', @PwdHash, 1, NULL, 1, GETDATE());

-- Tài khoản Nhân viên NV001
IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Username] = 'NV001')
    INSERT INTO [AppUsers] ([Username], [PasswordHash], [Role], [EmployeeId], [IsActive], [CreatedAt])
    VALUES ('NV001', @PwdHash, 2, @Emp1, 1, GETDATE());

-- Tài khoản Nhân sự NV002
IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Username] = 'NV002')
    INSERT INTO [AppUsers] ([Username], [PasswordHash], [Role], [EmployeeId], [IsActive], [CreatedAt])
    VALUES ('NV002', @PwdHash, 1, @Emp2, 1, GETDATE());

-- Tài khoản Kinh doanh NV003
IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Username] = 'NV003')
    INSERT INTO [AppUsers] ([Username], [PasswordHash], [Role], [EmployeeId], [IsActive], [CreatedAt])
    VALUES ('NV003', @PwdHash, 2, @Emp3, 1, GETDATE());

-- Tài khoản Quản trị NV004 (Võ Thị Quỳnh Như)
IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Username] = 'NV004')
    INSERT INTO [AppUsers] ([Username], [PasswordHash], [Role], [EmployeeId], [IsActive], [CreatedAt])
    VALUES ('NV004', @PwdHash, 0, @Emp4, 1, GETDATE());

-- Tài khoản NV005 (Lê Linh)
IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Username] = 'NV005')
    INSERT INTO [AppUsers] ([Username], [PasswordHash], [Role], [EmployeeId], [IsActive], [CreatedAt])
    VALUES ('NV005', @PwdHash, 2, @Emp5, 1, GETDATE());

PRINT N'>>> Đồng bộ cơ sở dữ liệu ChamCong_TinhLuong_DB thành công 100%!';
GO

USE [EmployeeManagementDB]
GO
/****** Object:  Table [dbo].[Employees]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employees](
	[EmployeeID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Designation] [nvarchar](100) NULL,
	[Address] [nvarchar](255) NULL,
	[Department] [nvarchar](100) NULL,
	[JoiningDate] [date] NULL,
	[Skillset] [nvarchar](500) NULL,
	[Username] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[Status] [nvarchar](20) NULL,
	[Role] [nvarchar](20) NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NULL,
	[ModifiedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[EmployeeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Employees] ON 
GO
INSERT [dbo].[Employees] ([EmployeeID], [Name], [Designation], [Address], [Department], [JoiningDate], [Skillset], [Username], [Password], [Status], [Role], [CreatedBy], [ModifiedBy], [CreatedAt], [ModifiedAt]) VALUES (1, N'admin', N'', N'', N'', NULL, N'Administration', N'admin', N'$2a$11$.IV7msbKf2MHpAiwJz/MDukivh0Mf.1BWLEd.abJI0.EROTg3HwOW', N'Active', N'Admin', N'Self', N'admin', CAST(N'2026-02-12T17:40:54.443' AS DateTime), CAST(N'2026-02-12T19:32:11.073' AS DateTime))
GO
INSERT [dbo].[Employees] ([EmployeeID], [Name], [Designation], [Address], [Department], [JoiningDate], [Skillset], [Username], [Password], [Status], [Role], [CreatedBy], [ModifiedBy], [CreatedAt], [ModifiedAt]) VALUES (2, N'user1', N'CEO', N'', N'Executive', NULL, N'Business Analytics', N'user1', N'$2a$11$55O7kk6exMyJaU.LI1hjrer1OrW7Aw0cmQ4Va7Olu11aFYrRUx.Di', N'Active', N'Employee', N'Self', N'admin', CAST(N'2026-02-12T18:45:50.993' AS DateTime), CAST(N'2026-02-13T11:21:23.530' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Employees] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Employee__536C85E4735B46B6]    Script Date: 13-02-2026 11.23.20 AM ******/
ALTER TABLE [dbo].[Employees] ADD UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Employees] ADD  DEFAULT ('Active') FOR [Status]
GO
ALTER TABLE [dbo].[Employees] ADD  DEFAULT ('Employee') FOR [Role]
GO
ALTER TABLE [dbo].[Employees] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Employees] ADD  DEFAULT (getdate()) FOR [ModifiedAt]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllEmployees]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 4. Get all employees (for admin)
CREATE PROCEDURE [dbo].[sp_GetAllEmployees]
AS
BEGIN
    SELECT * FROM Employees ORDER BY CreatedAt DESC;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetEmployeeById]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 3. Get employee by ID
CREATE PROCEDURE [dbo].[sp_GetEmployeeById]
    @EmployeeID INT
AS
BEGIN
    SELECT * FROM Employees WHERE EmployeeID = @EmployeeID;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetEmployeeByUsername]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- This procedure allows the C# code to fetch the HASHED password for verification
CREATE PROCEDURE [dbo].[sp_GetEmployeeByUsername]
    @Username NVARCHAR(50)
AS
BEGIN
    SELECT 
        EmployeeID, 
        Name, 
        Username, 
        Password, 
        Role, 
        Status 
    FROM Employees 
    WHERE Username = @Username;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoginEmployee]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_LoginEmployee]
    @Username NVARCHAR(50),
    @Password NVARCHAR(255)
AS
BEGIN
    SELECT EmployeeID, Name, Username, Role, Status
    FROM Employees
    WHERE Username = @Username AND Password = @Password;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_RegisterEmployee]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- 1. Register new employee
CREATE PROCEDURE [dbo].[sp_RegisterEmployee]
    @Name NVARCHAR(100),
    @Designation NVARCHAR(100),
    @Address NVARCHAR(255),
    @Department NVARCHAR(100),
    @JoiningDate DATE,
    @Skillset NVARCHAR(500),
    @Username NVARCHAR(50),
    @Password NVARCHAR(255),
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    INSERT INTO Employees (Name, Designation, Address, Department, JoiningDate, 
                          Skillset, Username, Password, CreatedBy, Role, Status)
    VALUES (@Name, @Designation, @Address, @Department, @JoiningDate, 
            @Skillset, @Username, @Password, @CreatedBy, 'Employee', 'Active');
    
    SELECT SCOPE_IDENTITY() AS EmployeeID;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateEmployee]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpdateEmployee]
    @EmployeeID INT,
    @Name NVARCHAR(100),
    @Designation NVARCHAR(100),
    @Address NVARCHAR(255),
    @Department NVARCHAR(100),
    @JoiningDate DATE,
    @Skillset NVARCHAR(500),
    @ModifiedBy NVARCHAR(100)
AS
BEGIN
    UPDATE Employees
    SET Name = @Name,
        Designation = @Designation,
        Address = @Address,
        Department = @Department,
        JoiningDate = @JoiningDate,
        Skillset = @Skillset,
        ModifiedBy = @ModifiedBy,
        ModifiedAt = GETDATE()
    WHERE EmployeeID = @EmployeeID;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateEmployeeStatus]    Script Date: 13-02-2026 11.23.20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_UpdateEmployeeStatus]
    @EmployeeID INT,
    @Status NVARCHAR(20),
    @ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Employees
    SET Status = @Status,
        ModifiedBy = @ModifiedBy,
        ModifiedAt = GETDATE()
    WHERE EmployeeID = @EmployeeID;

    SELECT @@ROWCOUNT AS RowsAffected;
END

GO





/****** Object:  View [dbo].[vwSMSList]    Script Date: 6/1/2022 12:13:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[Company](
	[CompanyId] [int] NOT NULL,
	[ParentId] [int] NULL,
	[Name] [varchar](200) NULL,
	[ShortName] [varchar](64) NULL,
	[OrderNo] [int] NOT NULL,
	[CompanyType] [int] NULL,
	[MushokNo] [nvarchar](50) NULL,
	[Address] [varchar](200) NULL,
	[Phone] [varchar](100) NULL,
	[Fax] [varchar](100) NULL,
	[Email] [varchar](100) NULL,
	[Controller] [varchar](100) NULL,
	[Action] [varchar](100) NULL,
	[Param] [varchar](250) NULL,
	[LayerNo] [int] NULL,
	[CompanyLogo] [varchar](150) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedBy] [varchar](50) NULL,
	[ModifiedDate] [datetime] NULL,
	[IsCompany] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Company] PRIMARY KEY CLUSTERED 
(
	[CompanyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_Company] FOREIGN KEY([ParentId])
REFERENCES [dbo].[Company] ([CompanyId])
GO

ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_Company]
GO




CREATE TABLE [dbo].[SmsType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_SmsType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SmsType] ADD  CONSTRAINT [DF_SmsType_IsActive]  DEFAULT ((0)) FOR [IsActive]
GO

CREATE TABLE [dbo].[ErpSMS](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Subject] [nvarchar](max) NULL,
	[PhoneNo] [nvarchar](11) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[CompanyId] [int] NOT NULL,
	[Status] [int] NULL,
	[TryCount] [int] NOT NULL,
	[RowTime] [datetime] NOT NULL,
	[Remarks] [nvarchar](max) NULL,
	[SmsType] [int] NOT NULL,
 CONSTRAINT [PK_ErpSMS] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ErpSMS] ADD  CONSTRAINT [DF_ErpSMS_TryCount]  DEFAULT ((0)) FOR [TryCount]
GO

ALTER TABLE [dbo].[ErpSMS] ADD  CONSTRAINT [DF_ErpSMS_RowTime]  DEFAULT (getdate()) FOR [RowTime]
GO

ALTER TABLE [dbo].[ErpSMS]  WITH CHECK ADD  CONSTRAINT [FK_ErpSMS_Company] FOREIGN KEY([CompanyId])
REFERENCES [dbo].[Company] ([CompanyId])
GO

ALTER TABLE [dbo].[ErpSMS] CHECK CONSTRAINT [FK_ErpSMS_Company]
GO

ALTER TABLE [dbo].[ErpSMS]  WITH CHECK ADD  CONSTRAINT [FK_ErpSMS_SmsType] FOREIGN KEY([SmsType])
REFERENCES [dbo].[SmsType] ([Id])
GO

ALTER TABLE [dbo].[ErpSMS] CHECK CONSTRAINT [FK_ErpSMS_SmsType]
GO

CREATE TABLE [dbo].[SMSScheduleLog](
	[SMSLogID] [bigint] IDENTITY(1,1) NOT NULL,
	[LogTime] [datetime] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Remarks] [nvarchar](max) NULL,
 CONSTRAINT [PK_SMSScheduleLog] PRIMARY KEY CLUSTERED 
(
	[SMSLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SMSScheduleLog] ADD  CONSTRAINT [DF_SMSScheduleLog_LogTime]  DEFAULT (getdate()) FOR [LogTime]
GO

ALTER TABLE [dbo].[SMSScheduleLog] ADD  CONSTRAINT [DF_SMSScheduleLog_Status]  DEFAULT (N'Success') FOR [Status]
GO

CREATE VIEW [dbo].[vwSMSList]
AS
SELECT        S.Id, S.Date, S.Subject, S.PhoneNo, S.Message, S.CompanyId, S.Status, S.TryCount, S.RowTime, S.Remarks, S.SmsType, ST.Name AS SMSTypeName, C.Name AS CompanyName
FROM            dbo.ErpSMS AS S INNER JOIN
                         dbo.SmsType AS ST ON S.SmsType = ST.Id INNER JOIN
                         dbo.Company AS C ON S.CompanyId = C.CompanyId
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "S"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ST"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 119
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C"
            Begin Extent = 
               Top = 6
               Left = 454
               Bottom = 136
               Right = 624
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSMSList'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSMSList'
GO

CREATE PROCEDURE [dbo].[SMSInfoUpdate]--	8, '31-07-2021'
	
	@ProcID nvarchar(2500) = '', 
	@Dxml01 xml = null,
	@Desc01 nvarchar(Max)=''
AS

SET NOCOUNT ON;

-- SET XACT_ABORT ON will cause the transaction to be uncommittable when the constraint violation occurs. 
	SET XACT_ABORT ON;
	SET QUERY_GOVERNOR_COST_LIMIT 0;
	begin
		begin try
			if @ProcID = 'UPDATESUCCESS01' goto UPDATESUCCESS01;

			if @ProcID = 'UPDATEFAILED01' goto UPDATEFAILED01;

			


			set @ProcID = char(13) + 'Process ID "' + @ProcID + '" not found to execute';
			raiserror(@ProcID, 16, 1);
		end try
		begin catch
			set @ProcID = 'UNKNOWN';
			execute dbo.SP_MOBILE_APP_UTILITY_INFO @ProcID = 'RETRIVEERRORINFO01', @Desc01 = @ProcID;
			end catch; 
		return;
	end;
return;

UPDATESUCCESS01:
BEGIN

   
BEGIN TRANSACTION [Tran1]

  BEGIN TRY

   select 
			ref.value('Id[1]', 'int') as Id,
			ref.value('Status[1]', 'int') as Status,
			ref.value('TryCount[1]', 'int') as TryCount
			into #tblUpdatedSms from @Dxml01.nodes('/ds/VwSmslist') xmlData(ref);

           
            UPDATE tb1
            SET tb1.Status=tb2.Status, TryCount=tb2.TryCount

            FROM dbo.ErpSMS AS tb1
            INNER JOIN #tblUpdatedSms AS tb2  ON tb1.Id = tb2.Id 

			INSERT INTO [dbo].[SMSScheduleLog]([LogTime],[Status],[Remarks])
			  VALUES(GETDATE(),'Success',@Desc01)
      COMMIT TRANSACTION [Tran1]
        select msg='Success' return 

  END TRY

  BEGIN CATCH

      ROLLBACK TRANSACTION [Tran1]
      execute dbo.SP_MOBILE_APP_UTILITY_INFO 'XXX', 'GETERRORINFO01', @Desc01=@ProcID;
  END CATCH      
END

UPDATEFAILED01:
BEGIN
	INSERT INTO [dbo].[SMSScheduleLog]([LogTime],[Status],[Remarks])
					 VALUES(GETDATE(),'Failed',@Desc01)
			
			begin
				select ref.value('Id[1]', 'int') as Id,
				ref.value('Status[1]', 'int') as Status,
				ref.value('TryCount[1]', 'int') as TryCount
				into #tblUpdatedSms2 from @Dxml01.nodes('/ds/VwSmslist') xmlData(ref);
			end
            UPDATE tb1
            SET tb1.Status=tb2.Status, TryCount=tb2.TryCount
            FROM dbo.ErpSMS AS tb1
            INNER JOIN #tblUpdatedSms2 AS tb2  ON tb1.Id = tb2.Id 
END
GO


CREATE TABLE [dbo].[BenchSession](
	[Id] [nvarchar](100) NOT NULL,
	[ParentId] [nvarchar](100) NULL,
	[Description] [nvarchar](max) NULL,
	[ThreadCount] [int] NULL,
	[TestMode] [int] NULL,
	[Runtimes] [int] NULL,
	[Duration] [int] NULL,
 CONSTRAINT [PK_BenchSession] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[BenchSession]  WITH CHECK ADD  CONSTRAINT [FK_BenchSession_BenchSession] FOREIGN KEY([ParentId])
REFERENCES [dbo].[BenchSession] ([Id])
GO

ALTER TABLE [dbo].[BenchSession] CHECK CONSTRAINT [FK_BenchSession_BenchSession]
GO
CREATE TABLE [dbo].[RunSession](
	[Id] [uniqueidentifier] NOT NULL,
	[SessionId] [nvarchar](100) NOT NULL,
	[InstanceTime] [datetime] NOT NULL,
 CONSTRAINT [PK_RunSession] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[RunSession]  WITH CHECK ADD  CONSTRAINT [FK_RunSession_RunSession] FOREIGN KEY([SessionId])
REFERENCES [dbo].[BenchSession] ([Id])
GO

ALTER TABLE [dbo].[RunSession] CHECK CONSTRAINT [FK_RunSession_RunSession]
GO
USE [Benchmark]
GO

/****** Object:  Table [dbo].[RunSessionResults]    Script Date: 7/8/2013 5:42:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RunSessionResults](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RunSessionId] [uniqueidentifier] NOT NULL,
	[Result] [int] NOT NULL,
 CONSTRAINT [PK_RunSessionResults] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[RunSessionResults]  WITH CHECK ADD  CONSTRAINT [FK_RunSessionResults_RunSession] FOREIGN KEY([RunSessionId])
REFERENCES [dbo].[RunSession] ([Id])
GO

ALTER TABLE [dbo].[RunSessionResults] CHECK CONSTRAINT [FK_RunSessionResults_RunSession]
GO


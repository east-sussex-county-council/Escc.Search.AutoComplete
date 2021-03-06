USE [master]
GO
/****** Object:  Database [SearchAutoComplete]    Script Date: 20/10/2015 17:54:28 ******/
CREATE DATABASE [SearchAutoComplete]
GO

USE [SearchAutoComplete]
GO
/****** Object:  Table [dbo].[InSearchKeywords]    Script Date: 20/10/2015 17:54:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[InSearchKeywords](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Keyword] [varchar](500) NOT NULL,
	[Pageviews] [int] NOT NULL,
	[FeedDate] [datetime] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[usp_InSearchKeywords_Delete]    Script Date: 20/10/2015 17:54:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		ESCC Web Team
-- Create date: 16/10/2011
-- Description:	Delete all the old keywords in preparation for the update
-- =============================================
CREATE PROCEDURE [dbo].[usp_InSearchKeywords_Delete]
AS
BEGIN TRAN

TRUNCATE TABLE InSearchKeywords
if @@error != 0
				BEGIN 
					Rollback Tran
					Return
				End
COMMIT TRAN

GO
/****** Object:  StoredProcedure [dbo].[usp_InSearchKeywords_Insert_Keyword]    Script Date: 20/10/2015 17:54:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		ESCC Web Team
-- Create date: 06/10/2011
-- Description:	Insert search keywords from data feed. The feed has already under
--				gone some data transformation to normalise the results and remove duplicates or bad words.
-- =============================================
CREATE PROCEDURE [dbo].[usp_InSearchKeywords_Insert_Keyword] 
	@Keyword varchar (500),
	@Pageviews int
AS
BEGIN TRAN

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	
	INSERT INTO 
		InSearchKeywords
		(
		Keyword,
		Pageviews
		)
		VALUES
		(
		@Keyword,
		@Pageviews
		)
		
if @@error != 0
				BEGIN 
					Rollback Tran
					Return
				End



COMMIT TRAN


GO
/****** Object:  StoredProcedure [dbo].[usp_InSearchKeywords_Select_BySearchTerm]    Script Date: 20/10/2015 17:54:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		ESCC Web Team
-- Create date: 06/10/2011
-- Description:	Select keywords based on search term entered by the user on the East Sussex County Council website.
--				The results are ordered by pageviews and consumed by the autocomplete search feature.
-- =============================================
CREATE PROCEDURE [dbo].[usp_InSearchKeywords_Select_BySearchTerm]
	@SearchTerm varchar (500)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP 10
		Keyword
	FROM
		InSearchKeywords
	WHERE Keyword LIKE @SearchTerm + '%'
	ORDER BY
		Pageviews DESC, Keyword ASC
		 
END


GO
USE [master]
GO
ALTER DATABASE [SearchAutoComplete] SET  READ_WRITE 
GO

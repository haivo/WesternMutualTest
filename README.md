## Functional Requirements
Design and implement WebAPI which performs CRUD to maintain a database of GUID 

1.	GUIDs are 32 hexadecimal characters, all uppercase.
2.	 The GUIDs are valid only for a limited period of time, with a default of 30 days from the time of creation, if an expiration time is not provided.
3.	The API only accepts valid JSON formatted data as input.
4.	The API will provide valid JSON formatted results.
5.	Only valid GUIDs are accessible by the API.
6.	The API should return appropriate HTTP code for each operation.
7.	Additional:
- Datetimes are Unix seconds.
- If GUID was expired, it means disabled/read only (does not allow to update metadata).  
- If you want to reuse expired GUID, it needs to be deleted and add new GUID. 
## Tech Stack
1.	.Net core 7 Minimal API
2.	SQL Server
3.	Playwright
4.	Git/GitHub
## Prerequisites
1.	Node.js LTS 18.16.0 (https://nodejs.org/en/download)
2.	Playwright engine. Install by running the following command 
`npx playwright install`
## Getting Started	
1. Create the `WesternMutual` database and update connection string in `appsettings.json`.
2. Run the following script to create `GuidMetadata` table
```sql
USE [WesternMutual]
GO


CREATE TABLE [dbo].[GuidMetadata](
	[Guid] [uniqueidentifier] NOT NULL,
	[Metadata] [nvarchar](max) NULL,
	[CreatedAt] [bigint] NOT NULL,
	[ExpiredAt] [bigint] NOT NULL,
	[UpdatedAt] [bigint] NOT NULL,
 CONSTRAINT [PK_GuidMetadata] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
```

3. Checking out the code, go to inside project `cd WesternMutual` and restore all nuget packages with `dotnet restore .\WesternMutual.sln`. 
4. Go to `Api` directory (ex: `C:\Home\GitRepository\WesternMutual\Api\`), start the api with command `dotnet run –urls=http://localhost:5116`. 
5. Open swagger at: `http://localhost:5116/swagger/index.html`.
Now you can test the Api with any REST client such as Postman or Swagger.


## Playwright Tests
If you run Api with different port then you need to update the port in file `Playwright\tests\guid.spec.ts` at line `5`
```ts
test.beforeAll(async ({ playwright }) => {
  apiContext = await playwright.request.newContext({
    baseURL: "http://localhost:5116",
    ignoreHTTPSErrors: true
  });
})
``` 



Setup Playwright.
1.	Go to Project `Playwright`’s directory 
(ex:  `C:\Home\GitRepository\WesternMutual\Playwright`)
2.	Run command to install packages: `npm install`
3.	Run command to run all test cases: `npx playwright test guid.spec.ts`
4. You should see the results like that:
```
Running 48 tests using 4 workers
  48 passed (22.8s)

To open last HTML report run:

  npx playwright show-report
  
```

 







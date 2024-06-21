# YT2PPT Converter
Youtube Video  to Power Point Converter

##  Clone the Repo
    git clone https://github.com/encryptedtouhid/YT2PPT.git

## Create appsettings.json

    {
      "Version": "1.3.0",
      "AppCpde": "YT2PP",
      "AppName": "Convert Youtube Video to PowerPoint",
      "AppOwner": "Khaled Md Tuhidul Hossain",
      "AppOwnerUrl": "https://tuhidulhossain.com",
      "YouTubeApiKey": "Some Key",
      "ConnectionStrings": {
           "DefaultConnection": "connection string"
       },
      "FreeLimit": "00:15:00",
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "Serilog": {
        "Using": [ "Serilog.Sinks.File" ],
        "MinimumLevel": {
          "Default": "Error"
        },
        "WriteTo": [
          {
            "Name": "MSSqlServer",
            "Args": {
              "connectionString": "connection string",
              "tableName": "Logs",
              "autoCreateSqlTable": true
            }
          }
        ]
      }
    }

## Clean & Build
  Open Visual Studio and build the project. It will restore all the nuget package needed.

## Output
![image](https://github.com/encryptedtouhid/YT2PPT/assets/10276184/3908fac4-0865-4f32-9381-aba92e6e3a99)



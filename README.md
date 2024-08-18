# YT2PPT Converter
Youtube Video  to Power Point Converter

##  Clone the Repo
    git clone https://github.com/encryptedtouhid/YT2PPT.git

## Create appsettings.json

    {
      "Version": "1.4.0",
      "AppCpde": "YT2PP",
      "AppName": "Convert Youtube Video to PowerPoint",
      "AppOwner": "Khaled Md Tuhidul Hossain",
      "AppOwnerUrl": "https://tuhidulhossain.com",
      "YouTubeApiKey": "[KEY]",
      "ConnectionStrings": {
        "DefaultConnection": "[DB CONNECTION STRING]"
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
        "Using": [ "Serilog.Sinks.File", "Serilog.Sinks.MSSqlServer" ],
        "MinimumLevel": {
          "Default": "Error"
        },
        "WriteTo": [
          {
            "Name": "File",
            "Args": {
              "path": "[TEXT FILE NAME]",
              "rollingInterval": "Day"
            }
          },
          {
            "Name": "MSSqlServer",
            "Args": {
              "connectionString": "[DB CONNECTION STRING]",
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



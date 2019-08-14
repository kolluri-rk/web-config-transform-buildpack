## Web Config Transform Buildpack

### Why do you need this recipe?

Cloud Native Applications are expected to bring in configurations from external sources like environment variables, config server, etc. Please refer to `Configuration` in [12factor.net](https://12factor.net) for more information.

In legacy ASP.Net applications, configuration settings are injected through web.config files. As per cloud native principles, configuration should stay out of build artifacts. In this recipe we will use a custom buildpack which provides a solution to this problem by using token replacement during cf push staging.

### High level steps

1. Identify environment dependant configurations and externalize
1. Create app manifest
1. Add web config transformations
1. Move config settings to git repository
1. Create config server service
1. Bind config service to app using manifest
1. Push app by parameterized environment name

#### 1. Identify environment dependant configurations and externalize

* Identify configuration items (in web.config files) that are environment dependant that need to be externalized.
* Tokenize the configuration item in the following format #{configPath:key}. For e.g. please refer before and after as below

##### BEFORE

```xml
<connectionStrings>
    <add name="MySqlDb" connectionString="Data Source=xxxx;Initial Catalog=xxxx;User ID=xxxx;Password=xxxx" providerName="System.Data.SqlClient" />
</connectionStrings>

```

##### AFTER

```xml
<connectionStrings>
    <add name="MySqlDb" connectionString="#{connectionStrings:MySqlDb}" providerName="System.Data.SqlClient" />
</connectionStrings>

``` 

#### 2. Create app manifest  

* Ensure your application has a pcf manifest file. If your application is in Cloud Foundry already, you can create the manifest using the command `cf create-app-manifest [appname]`. 
* Add a buildpack reference to the manifest (before the hwc buildpack) that will perform the token replacement on cf push action.  
    >Note: Please refer to https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases to pull the latest version as appropriate. `XXX` refers to the version of buildpack.  
* Add an environment variable to the manifest for each config item that will be used to replace the tokenized values. Below is a sample added refering to the connection string. 
    >Note: Adding token replacements with Environment variables is only for experimental activities. Config settings should be externalized using git repos and Spring Cloud Config Server.


```yaml
applications:
- name: sampleapp
buildpacks:
- https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases/download/XXX/web-config-transform-buildpack-XXX.zip
- hwc_buildpack
env:
    "connectionStrings:MySqlDb": "Data Source=xxxx;Initial Catalog=xxxx;User ID=xxxx;Password=xxxx"
```

##### Note  
> Only put token environment variables in the manifest for testing purposes. Spring Cloud Config Server should be used for externalizing configuration settings.


#### 3. Add web config transformations

By default, all web apps and wcf apps created with **Debug** and **Release** configurations and corresponding web config transformation files (web.Debug.config, web.Release.config).

* Add required transformations to `web.Release.Config`

* Build and push the application to pcf to verify that your config settings are properly transformed

###### Note
> For developer machine debugging, use `Debug` configuration profile and `Web.Debug.config` for transformation.


Sample `web.Release.Config` with transformations  

```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- For PCF -->
<configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
  
  <connectionStrings  xdt:Transform="Replace">
    <add name="MySqlDb" connectionString="#{connectionStrings:MySqlDb}" providerName="System.Data.SqlClient"/>
  </connectionStrings>
  
  <system.serviceModel>
    <client xdt:Transform="Replace">
      
      <endpoint 
        address="#{client:Default_IMyLogService:address}" 
        binding="#{client:Default_IMyLogService:binding}" 
        bindingConfiguration="#{client:Default_IMyLogService:bindingConfiguration}"
        contract="ServiceProxy.IMyLogService" name="Default_IMyLogService" />
    
    </client>
  </system.serviceModel>

</configuration>
```

#### 4. Move config settings to git repository

1. Create git repository per application
1. Create <YOUR_APPLICATION>.yaml file to have common settings across all environments
1. Create <YOUR_APPLICATION>-<ENVIRONMENT>.yaml for each environment
1. Specify your environment  with `ASPNETCORE_ENVIRONMENT` environment variable in the manifest file. e.g: `ASPNETCORE_ENVIRONMENT=Production`

##### Sample yaml files

sampleapp.yaml
```yaml
appSettings:
  Setting1: "Common setting1"
```

sampleapp-Development.yaml
```yaml
 connectionStrings:
   MySqlDb: "Data Source=devserver;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"
```

sampleapp-Production.yaml
```yaml
 connectionStrings:
   MySqlDb: "Data Source=prodserver;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"
```

#### 5. Create config server service

1. Make sure you have config server available in marketplace. 

1. Create a json file for config srever setup (config-server.json)
    ```json
    {
        "git" : { 
            "uri": "https://github.com/YOUR_USERNAME/YOUR_APPREPO"
        }

    }
    ```
1. Create config server using above configuration file.
    ```script
    cf create-service p-config-server standard my_configserver  -c .\config-server.json
    ```

#### 6. Bind config service to app using manifest

Bind config server to your app once config server service is created.

```yaml
---
applications:
- name: sampleapp
  stack: windows
  buildpacks: 
    - https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases/download/v1.1.5/Pivotal.Web.Config.Transform.Buildpack-win-x64-1.1.5.zip
    - hwc_buildpack
  env:
    ASPNETCORE_ENVIRONMENT: ((env))

  services:
  - my_configserver
```

#### 7. Push app by parameterized environment name

Parameterizing your application environment gives ability to provide different value as per you deploy stage in CD pipelines. e.g: Development/QA/UAT/Production.

This can be achieved by replacing hardcoded value of `ASPNETCORE_ENVIRONMENT=YOUR_DEPLOY_ENVIRONMENT` with `ASPNETCORE_ENVIRONMENT: ((env))`

Now you can push your app with below command

```script
cf push --var env=Production
```

You should be able to find if the environment value is actually passed in, by looking at logs.

```
================================================================================
=============== WebConfig Transform Buildpack execution started ================
================================================================================
-----> Using Environment: Production
-----> Config server binding found...
```


#### Note
> For any issues you face with the web-config-transform-buildpack, please raise an issue at https://github.com/cloudfoundry-community/web-config-transform-buildpack/issues.


## Extra stuff (TBD)
* Describe how to use separate config transform ie ASPNETCORE_ENVIRONMENT=<env> + Web.<env>.config
* Describe optional special behavior for appSettings and connectionStrings

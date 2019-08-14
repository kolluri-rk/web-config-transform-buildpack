## Web Config Transform Buildpack

### Why do you need this recipe?

Cloud Native Applications are expected to bring in configurations from external sources like environment variables, config server, etc. Please refer to `Configuration` in [12factor.net](https://12factor.net) for more information

In legacy ASP.Net applications, configuration are injected through web.config files. As per cloud native priciples, configuration should stay out of build artifacts. In this recipe we will use a custom buildpack which provides a solution to this problem by using token replacement during cf push staging.

### High level steps

1. Identify environment dependant configurations and externalize
1. Create app manifest
1. Add web config transformations

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

* Ensure your application has a pcf manifest file. If your application is in PCF already, you can create the manifest using the command `cf create-app-manifest [appname]`. Sample manifest below.  
* Add an environment variable to the manifest for each config item that will be used to replace the tokenized values. Below is a sample added refering to the connection string example above.
* Add a buildpack reference to the manifest (before the hwc buildpack) that will perform the token replacement on cf push action. Note: Please refer to https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases to pull the latest version as appropriate. `XXX` refers to the version of buildpack.


```yaml
applications:
- name: sampleapp
buildpacks:
- https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases/download/XXX/web-config-transform-buildpack-XXX.zip
- hwc_buildpack
disk_quota: 1G
instances: 1
env:
    ASPNETCORE_ENVIRONMENT: Production
    "connectionStrings:MySqlDb": "Data Source=xxxx;Initial Catalog=xxxx;User ID=xxxx;Password=xxxx"
```



##### Note  
> On any issues you face with the web-config-transform-buildpack, please raise an issue at https://github.com/cloudfoundry-community/web-config-transform-buildpack/issues.


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



## Extra stuff (TBD)
* Describe how to use separate config transform ie ASPNETCORE_ENVIRONMENT=<env> + Web.<env>.config
* Describe optional special behavior for appSettings and connectionStrings

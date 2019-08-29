﻿using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Pivotal.Web.Config.Transform.Buildpack;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class WebConfigTransformStartupTests
    {
        private readonly IConfigurationRoot _configMock;
        private readonly Mock<IWebConfigReader> _webConfigReaderMock;
        private readonly Mock<IWebConfigWriter> _webConfigWriterMock;
        private readonly WebConfigTransformStartup _transformStartup; 

        public WebConfigTransformStartupTests()
        {
            _configMock = GetMockConfiguration();
            _webConfigReaderMock = new Mock<IWebConfigReader>();
            _webConfigWriterMock = new Mock<IWebConfigWriter>();

            _transformStartup = new WebConfigTransformStartup(_configMock, _webConfigReaderMock.Object);
        }

        private IConfigurationRoot GetMockConfiguration()
        {
            var externalAppSettings = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("appSettings:BP_AppSettings_Key1", "External_AppSettings_Value1"),
                new KeyValuePair<string, string>("connectionStrings:BP_ConnectionStrings_Key1", "External_ConnectionStrings_Value1"),
                new KeyValuePair<string, string>("BP_Token1", "External_BP_Token1_Value"),
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(externalAppSettings).Build();
            return config;
        }

        [Fact]
        public void When_ConfigIsNull_Constructor_Should_ThrowException()
        {
            Action startup = () => { new WebConfigTransformStartup(null, _webConfigReaderMock.Object); };

            startup.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_WebConfigReaderIsNull_Constructor_Should_ThrowException()
        {
            Action startup = () => { new WebConfigTransformStartup(_configMock, null); };

            startup.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_WebConfigWriterIsNull_CopyExternalAppSettings_Should_ThrowException()
        {
            Action useAppSettings = () => { _transformStartup.CopyExternalAppSettings(null); };

            useAppSettings.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_ExternalConfigHasAppSetting_CopyExternalAppSettings_Should_CallSetAppSetting()
        {
            var configAppSettings = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("BP_AppSettings_Key1", "AppSettings_Value1"),
                new KeyValuePair<string, string>("BP_AppSettings_Key2", "AppSettings_Value2"),
            };
            _webConfigReaderMock.Setup(r => r.GetAppSettings()).Returns(configAppSettings);

            _transformStartup.CopyExternalAppSettings(_webConfigWriterMock.Object);

            _webConfigWriterMock.Verify(w => w.SetAppSetting(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void When_ExternalConfigHasConnectionStrings_CopyExternalConnectionStrings_Should_CallSetConnectionString()
        {
            var configConnectionStrings = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("BP_ConnectionStrings_Key1", "ConnectionStrings_Value1"),
                new KeyValuePair<string, string>("BP_ConnectionStrings_Key2", "ConnectionStrings_Value2"),
            };
            _webConfigReaderMock.Setup(r => r.GetConnectionStrings()).Returns(configConnectionStrings);

            _transformStartup.CopyExternalConnectionStrings(_webConfigWriterMock.Object);

            _webConfigWriterMock.Verify(w => w.SetConnectionString(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void When_ExternalConfigHasTokens_CopyExternalTokens_Should_CallInitializeReplaceFinalize()
        {
            _transformStartup.CopyExternalTokens(_webConfigWriterMock.Object);

            _webConfigWriterMock.Verify(w => w.InitializeWebConfigForTokenReplacements(), Times.Once);
            _webConfigWriterMock.Verify(w => w.ReplaceToken(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            _webConfigWriterMock.Verify(w => w.FinalizeWebConfigTokenReplacements(), Times.Once);
        }
    }
}

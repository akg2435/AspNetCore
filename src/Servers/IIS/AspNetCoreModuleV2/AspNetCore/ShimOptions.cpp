// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

#include "ShimOptions.h"

#include "StringHelpers.h"
#include "ConfigurationLoadException.h"
#include "Environment.h"

#define CS_ASPNETCORE_HANDLER_VERSION                    L"handlerVersion"

ShimOptions::ShimOptions(const ConfigurationSource &configurationSource) :
        m_hostingModel(HOSTING_UNKNOWN),
        m_fStdoutLogEnabled(false)
{
    auto const section = configurationSource.GetRequiredSection(CS_ASPNETCORE_SECTION);
    auto hostingModel = section->GetString(CS_ASPNETCORE_HOSTING_MODEL).value_or(L"");

    if (hostingModel.empty() || equals_ignore_case(hostingModel, CS_ASPNETCORE_HOSTING_MODEL_OUTOFPROCESS))
    {
        m_hostingModel = HOSTING_OUT_PROCESS;
    }
    else if (equals_ignore_case(hostingModel, CS_ASPNETCORE_HOSTING_MODEL_INPROCESS))
    {
        m_hostingModel = HOSTING_IN_PROCESS;
    }
    else
    {
        throw ConfigurationLoadException(format(
            L"Unknown hosting model '%s'. Please specify either hostingModel=\"inprocess\" "
            "or hostingModel=\"outofprocess\" in the web.config file.", hostingModel.c_str()));
    }

    if (m_hostingModel == HOSTING_OUT_PROCESS)
    {
        const auto handlerSettings = section->GetKeyValuePairs(CS_ASPNETCORE_HANDLER_SETTINGS);
        m_strHandlerVersion = find_element(handlerSettings, CS_ASPNETCORE_HANDLER_VERSION).value_or(std::wstring());
    }

    m_strProcessPath = section->GetRequiredString(CS_ASPNETCORE_PROCESS_EXE_PATH);
    m_strArguments = section->GetString(CS_ASPNETCORE_PROCESS_ARGUMENTS).value_or(CS_ASPNETCORE_PROCESS_ARGUMENTS_DEFAULT);
    m_fStdoutLogEnabled = section->GetRequiredBool(CS_ASPNETCORE_STDOUT_LOG_ENABLED);
    m_struStdoutLogFile = section->GetRequiredString(CS_ASPNETCORE_STDOUT_LOG_FILE);
    m_fDisableStartupPage = section->GetRequiredBool(CS_ASPNETCORE_DISABLE_START_UP_ERROR_PAGE);

    const auto detailedErrors = Environment::GetEnvironmentVariableValue(L"ASPNETCORE_DETAILEDERRORS").value_or(L"");
    const auto aspnetCoreEnvironment = Environment::GetEnvironmentVariableValue(L"ASPNETCORE_ENVIRONMENT").value_or(L"");
    const auto dotnetEnvironment = Environment::GetEnvironmentVariableValue(L"DOTNET_ENVIRONMENT").value_or(L"");

    auto detailedErrorsEnabled = equals_ignore_case(L"1", detailedErrors) || equals_ignore_case(L"true", detailedErrors);
    auto aspnetCoreEnvironmentEnabled = equals_ignore_case(L"Development", aspnetCoreEnvironment);
    auto dotnetEnvironmentEnabled = equals_ignore_case(L"Development", dotnetEnvironment);

    // Need to read web.config environment variables to check if aspnetcore environment is set too.
    auto environmentVariables = section->GetMap(CS_ASPNETCORE_ENVIRONMENT_VARIABLES);

    // Technically this will add an entry to the map here, but we aren't keeping the m
    const auto detailedErrorsFromWebConfig = environmentVariables[L"ASPNETCORE_DETAILEDERRORS"];
    const auto aspnetCoreEnvironmentFromWebConfig = environmentVariables[L"ASPNETCORE_ENVIRONMENT"];
    const auto dotnetEnvironmentFromWebConfig = environmentVariables[L"DOTNET_ENVIRONMENT"];

    // TODO make some helpers for this
    detailedErrorsEnabled |= equals_ignore_case(L"1", detailedErrorsFromWebConfig) || equals_ignore_case(L"true", detailedErrorsFromWebConfig);
    aspnetCoreEnvironmentEnabled |= equals_ignore_case(L"Development", aspnetCoreEnvironmentFromWebConfig);
    dotnetEnvironmentEnabled |= equals_ignore_case(L"Development", dotnetEnvironmentFromWebConfig);

    m_fShowDetailedErrors = detailedErrorsEnabled || aspnetCoreEnvironmentEnabled || dotnetEnvironmentEnabled;
}

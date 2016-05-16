# Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
# Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

[cmdletbinding(SupportsShouldProcess=$true)]
param($publishProperties=@{}, $packOutput, $pubProfilePath, $nugetUrl)

# to learn more about this file visit https://go.microsoft.com/fwlink/?LinkId=524327
$publishModuleVersion = '1.1.0'

function Get-PublishModulePath{
    [cmdletbinding()]
    param()
    process{
        $keysToCheck = @('hklm:\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\{0}',
                         'hklm:\SOFTWARE\Microsoft\VisualStudio\{0}',
                         'hklm:\SOFTWARE\Wow6432Node\Microsoft\VWDExpress\{0}',
                         'hklm:\SOFTWARE\Microsoft\VWDExpress\{0}'
                         )
        $versions = @('14.0', '15.0')

        [string]$publishModulePath=$null
        :outer foreach($keyToCheck in $keysToCheck){
            foreach($version in $versions){
                if(Test-Path ($keyToCheck -f $version) ){
                    $vsInstallPath = (Get-itemproperty ($keyToCheck -f $version) -Name InstallDir -ErrorAction SilentlyContinue | select -ExpandProperty InstallDir -ErrorAction SilentlyContinue)
                    
                    if($vsInstallPath){
                        $installedPublishModulePath = "{0}Extensions\Microsoft\Web Tools\Publish\Scripts\{1}\" -f $vsInstallPath, $publishModuleVersion
                        if(!(Test-Path $installedPublishModulePath)){
                            $vsInstallPath = $vsInstallPath + 'VWDExpress'
                            $installedPublishModulePath = "{0}Extensions\Microsoft\Web Tools\Publish\Scripts\{1}\" -f  $vsInstallPath, $publishModuleVersion
                        }
                        if(Test-Path $installedPublishModulePath){
                            $publishModulePath = $installedPublishModulePath
                            break outer;
                        }
                    }
                }
            }
        }

        $publishModulePath
    }
}

$publishModulePath = Get-PublishModulePath

$defaultPublishSettings = New-Object psobject -Property @{
    LocalInstallDir = $publishModulePath
}

function Enable-PackageDownloader{
    [cmdletbinding()]
    param(
        $toolsDir = "$env:LOCALAPPDATA\Microsoft\Web Tools\Publish\package-downloader-$publishModuleVersion\",
        $pkgDownloaderDownloadUrl = 'https://go.microsoft.com/fwlink/?LinkId=524325') # package-downloader.psm1
    process{
        if(get-module package-downloader){
            remove-module package-downloader | Out-Null
        }

        if(!(get-module package-downloader)){
            if(!(Test-Path $toolsDir)){ New-Item -Path $toolsDir -ItemType Directory -WhatIf:$false }

            $expectedPath = (Join-Path ($toolsDir) 'package-downloader.psm1')
            if(!(Test-Path $expectedPath)){
                'Downloading [{0}] to [{1}]' -f $pkgDownloaderDownloadUrl,$expectedPath | Write-Verbose
                (New-Object System.Net.WebClient).DownloadFile($pkgDownloaderDownloadUrl, $expectedPath)
            }
        
            if(!$expectedPath){throw ('Unable to download package-downloader.psm1')}

            'importing module [{0}]' -f $expectedPath | Write-Output
            Import-Module $expectedPath -DisableNameChecking -Force
        }
    }
}

function Enable-PublishModule{
    [cmdletbinding()]
    param()
    process{
        if(get-module publish-module){
            remove-module publish-module | Out-Null
        }

        if(!(get-module publish-module)){
            $localpublishmodulepath = Join-Path $defaultPublishSettings.LocalInstallDir 'publish-module.psm1'
            if(Test-Path $localpublishmodulepath){
                'importing module [publish-module="{0}"] from local install dir' -f $localpublishmodulepath | Write-Verbose
                Import-Module $localpublishmodulepath -DisableNameChecking -Force
                $true
            }
        }
    }
}

try{

    if (!(Enable-PublishModule)){
        Enable-PackageDownloader
        Enable-NuGetModule -name 'publish-module' -version $publishModuleVersion -nugetUrl $nugetUrl
    }

    'Calling Publish-AspNet' | Write-Verbose
    # call Publish-AspNet to perform the publish operation
    Publish-AspNet -publishProperties $publishProperties -packOutput $packOutput -pubProfilePath $pubProfilePath
}
catch{
    "An error occurred during publish.`n{0}" -f $_.Exception.Message | Write-Error
}
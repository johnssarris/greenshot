param(
    [Parameter(Mandatory=$true)]
    [string]$RepositoryRootPath,
    [Parameter(Mandatory=$true)]
    [string]$BuildArtifactsPath,
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

# Create portable directory
New-Item -ItemType Directory -Path "$OutputPath" -Force | Out-Null

# Copy greenshot.exe
Copy-Item "$BuildArtifactsPath\Greenshot.exe" "$OutputPath" -Force
# Copy greenshot.exe.config
Copy-Item "$BuildArtifactsPath\Greenshot.exe.config" "$OutputPath" -Force

# Copy all dlls
Copy-Item "$BuildArtifactsPath\*.dll" "$OutputPath" -Force

# Copy emoji resources
Copy-Item "$BuildArtifactsPath\emojis.xml" "$OutputPath" -Force
Copy-Item "$BuildArtifactsPath\Twemoji.Mozilla.ttf" "$OutputPath" -Force

# Copy help files
New-Item -ItemType Directory -Path "$OutputPath\Help" -Force | Out-Null
Copy-Item "$RepositoryRootPath\src\Greenshot\Languages\*.html" "$OutputPath\Help" -Force

# Copy languages files
New-Item -ItemType Directory -Path "$OutputPath\Languages" -Force | Out-Null
Copy-Item "$RepositoryRootPath\src\Greenshot\Languages\*.xml" "$OutputPath\Languages" -Force

# Create Dummy-INI
";dummy config, used to make greenshot store the configuration in this directory" | Set-Content "$OutputPath\greenshot.ini" -Encoding UTF8

# Create Dummy-defaults-INI
";In this file you should add your default settings" | Set-Content "$OutputPath\greenshot-defaults.ini" -Encoding UTF8

# Create Dummy-fixed-INI
";In this file you should add your fixed settings" | Set-Content "$OutputPath\greenshot-fixed.ini" -Encoding UTF8

# Copy license file
Copy-Item "$RepositoryRootPath\LICENSE" "$OutputPath\license.txt" -Force

# Copy and rename log config file
Copy-Item "$RepositoryRootPath\src\Greenshot\log4net-zip.xml" "$OutputPath\log4net.xml" -Force

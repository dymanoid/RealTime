function Test-XmlFile {

    [CmdletBinding()]
    param (
        [parameter(mandatory=$true)][ValidateNotNullorEmpty()][string]$xmlFilePath
    )

    $xml = New-Object System.Xml.XmlDocument
    try {
        $xml.Load((Get-ChildItem -Path $xmlFilePath).FullName)
        Write-Host "XML file is valid: $xmlFilePath"
        return $true
    }
    catch [System.Xml.XmlException] {
        Write-Error "XML file is invalid: $xmlFilePath --> $($_.toString())"
        return $false
    }
}

function Test-XmlFiles {

    [CmdletBinding()]
    param (
        [parameter(mandatory=$true)][ValidateNotNullorEmpty()][string]$filesPath
    )

    if (!(Test-Path -Path $filesPath)){
        throw "$filesPath is not valid. Please provide a valid path"
    }

    $files = Get-ChildItem -Path $filesPath -Recurse -Include *.xml
    $result = $true

    foreach ($f in $files) {
        $fileResult = Test-XmlFile $f.FullName
        if (!$fileResult) {
            $result = $false
        }
    }

    return $result
}

$scriptResult = Test-XmlFiles $args[0]
if ($scriptResult) {
    exit 0
}
else {
    exit 1
}
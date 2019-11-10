function Test-XmlFile {
    <#
    .SYNOPSIS
    Test the validity of an XML file
    #>

    [CmdletBinding()]
    param (
    [parameter(mandatory=$true)][ValidateNotNullorEmpty()][string]$xmlFilePath
    )
   
    if (!(Test-Path -Path $xmlFilePath)){
        throw "$xmlFilePath is not valid. Please provide a valid path to the .xml fileh"
    }

    $xml = New-Object System.Xml.XmlDocument
    try {
        $xml.Load((Get-ChildItem -Path $xmlFilePath).FullName)
        return $true
    }
    catch [System.Xml.XmlException] {
        Write-Verbose "$xmlFilePath : $($_.toString())"
        return $false
    }
}

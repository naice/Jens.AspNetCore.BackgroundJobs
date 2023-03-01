if ($args.Count -eq 0)
{
    Write-Output("No parameter specified.");
    exit;
}

$cmd = $args[0];
$cmd = "test"

if ($cmd -eq "test")
{
    
    Remove-Item -r ./TestReport; 
    Remove-Item -r ./TestResults;
    dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults;
    reportgenerator "-reports:TestResults/*/*.xml" "-targetdir:TestReport";
    Start-Process ./TestReport/index.htm
}



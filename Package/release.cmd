"..\..\oqtane.framework\oqtane.package\nuget.exe" pack Oqtane.Blogs.nuspec 
XCOPY "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\" /Y

//Enable/Disable stats to be exported into html at the end of each game
$StatsExport::Enabled = true;

//load all related files
exec("code.containers.cs");
exec("code.stack.cs");
exec("code.marker.cs");
exec("code.exporter.cs");
exec("code.collector.cs");
exec("code.zbridge.cs" );
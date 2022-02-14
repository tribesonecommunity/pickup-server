function Exporter::ResetTotals(%teamname)
{
    $Collector::Score[%teamname] = 0;
    $Collector::Kills[%teamname] = 0;
    $Collector::Deaths[%teamname] = 0;
    $Collector::Suicides[%teamname] = 0;
    $Collector::TeamKills[%teamname] = 0;
    $Collector::TeamDeaths[%teamname] = 0;
    $Collector::Kills[%teamname, "Disc"] = 0;
    $Collector::Deaths[%teamname, "Disc"] = 0;
    $Collector::Kills[%teamname, "Explosive"] = 0;
    $Collector::Deaths[%teamname, "Explosive"] = 0;
    $Collector::Kills[%teamname, "Chaingun"] = 0;
    $Collector::Deaths[%teamname, "Chaingun"] = 0;
    $Collector::CarrierKills[%teamname] = 0;
    $Collector::MidAirCK[%teamname] = 0;
    $Collector::Returns[%teamname] = 0;
    $Collector::StandoffReturns[%teamname] = 0;
    $Collector::Interceptions[%teamname] = 0;
    $Collector::BBGiven[%teamname] = 0;
    $Collector::BBTaken[%teamname] = 0;
    $Collector::Grabs[%teamname] = 0;
    $Collector::Pickups[%teamname] = 0;
    $Collector::Catches[%teamname] = 0;
    $Collector::Drops[%teamname] = 0;
    $Collector::Assists[%teamname] = 0;
    $Collector::Caps[%teamname] = 0;
    $Collector::MAGiven[%teamname] = 0;
    $Collector::MATaken[%teamname] = 0;
    $Collector::DamageOut[%teamname] = 0;
    $Collector::DamageIn[%teamname] = 0;
    $Collector::TeamDamageOut[%teamname] = 0;
    $Collector::TeamDamageIn[%teamname] = 0;
    $Collector::NadeJumps[%teamname] = 0;
    $Collector::MANades[%teamname] = 0;
    $Collector::Craters[%teamname] = 0;
    $Collector::ClutchReturns[%teamname] = 0;
    $Collector::EGrabs[%teamname] = 0;
    $Collector::Clunks[%teamname] = 0;
    $Collector::timeflag[ 0, %teamname ] = 0;
    $Collector::timeflag[ 1, %teamname ] = 0;
}

function Exporter::ActualTotals(%name, %teamname)
{
    
    $Collector::Score[%teamname] += $Collector::Score[%name];
    $Collector::Kills[%teamname] += $Collector::Kills[%name];
    $Collector::Deaths[%teamname] += $Collector::Deaths[%name];
    $Collector::Suicides[%teamname] += $Collector::Suicides[%name];
    $Collector::TeamKills[%teamname] += $Collector::TeamKills[%name];
    $Collector::TeamDeaths[%teamname] += $Collector::TeamDeaths[%name];
    $Collector::Kills[%teamname, "Disc"] += $Collector::Kills[%name, "Disc"];
    $Collector::Deaths[%teamname, "Disc"] += $Collector::Deaths[%name, "Disc"];
    $Collector::Kills[%teamname, "Explosive"] += $Collector::Kills[%name, "Explosive"];
    $Collector::Deaths[%teamname, "Explosive"] += $Collector::Deaths[%name, "Explosive"];
    $Collector::Kills[%teamname, "Chaingun"] += $Collector::Kills[%name, "Chaingun"];
    $Collector::Deaths[%teamname, "Chaingun"] += $Collector::Deaths[%name, "Chaingun"];
    $Collector::CarrierKills[%teamname] += $Collector::CarrierKills[%name];
    $Collector::MidAirCK[%teamname] += $Collector::MidAirCK[%name];
    $Collector::Returns[%teamname] += $Collector::Returns[%name];
    $Collector::StandoffReturns[%teamname] += $Collector::StandoffReturns[%name];
    $Collector::Interceptions[%teamname] += $Collector::Interceptions[%name];
    $Collector::BBGiven[%teamname] += $Collector::BBGiven[%name];
    $Collector::BBTaken[%teamname] += $Collector::BBTaken[%name];
    $Collector::Grabs[%teamname] += $Collector::Grabs[%name];
    $Collector::Pickups[%teamname] += $Collector::Pickups[%name];
    $Collector::Catches[%teamname] += $Collector::Catches[%name];
    $Collector::Drops[%teamname] += $Collector::Drops[%name];
    $Collector::Assists[%teamname] += $Collector::Assists[%name];
    $Collector::Caps[%teamname] += $Collector::Caps[%name];
    $Collector::MAGiven[%teamname] += $Collector::MAGiven[%name];
    $Collector::MATaken[%teamname] += $Collector::MATaken[%name];
    $Collector::DamageOut[%teamname] += $Collector::DamageOut[%name];
    $Collector::DamageIn[%teamname] += $Collector::DamageIn[%name];
    $Collector::TeamDamageOut[%teamname] += $Collector::TeamDamageOut[%name];
    $Collector::TeamDamageIn[%teamname] += $Collector::TeamDamageIn[%name];
    $Collector::NadeJumps[%teamname] += $Collector::NadeJumps[%name];
    $Collector::MANades[%teamname] += $Collector::MANades[%name];
    $Collector::Craters[%teamname] += $Collector::Craters[%name];
    $Collector::ClutchReturns[%teamname] += $Collector::ClutchReturns[%name];
    $Collector::EGrabs[%teamname] += $Collector::EGrabs[%name];
    $Collector::Clunks[%teamname] += $Collector::Clunks[%name];
    $Collector::TotalFlagTime[ %teamname ] += ($Collector::timeflag[ 0, %name ] + $Collector::timeflag[ 1, %name ]);
}

function Exporter::FinalScore() {

    for ( %team = 0; %team <= 1; %team++ ) {
        
        %teamname = Marker::Team( %team );
        Exporter::ResetTotals( %teamname );
        Stack::Reset( "teamlist" @ %team );
        
        for ( %j = 0; %j < Stack::Count( "teamlist" @ %team ); %j++ ) {
            
            %name = Stack::GetNext( "teamlist" @ %team );
            Exporter::ActualTotals(%name, %teamname);
            
        }
    }

    %teamName0 = Marker::Team( 0 );
    %teamName1 = Marker::Team( 1 );
    
    //BE wins
    if ($Collector::Caps[%teamName0] > $Collector::Caps[%teamName1]) { return "BE"; }
    //DS wins
    else if ($Collector::Caps[%teamName0] < $Collector::Caps[%teamName1]) { return "DS"; }
    //tie game
    else { return "TIE"; }
    
}

function Exporter::ExportMap( %name ) {
	echo( "EXPORTING " @ %name );
    
	$Collector::GameOutcome = Exporter::FinalScore();

    export( "$Collect*", "Temp/collector.cs" );

}
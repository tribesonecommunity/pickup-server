exec( "code.html.cs" );

//function String::Length(%str) {
  //if (String::Empty(%str)) {
    //return 0;
  //}
  //for (%i = 0; !String::Empty(String::GetSubStr(%str, %i, 1)); %i++) {
    // NOOP
  //}
  //return %i;
//}

function String::lPad(%input, %length, %pad) {
  if (!%length || String::len(%input) >= %length) {
    return %input;
  }
  if (String::Empty(%pad)) {
    %pad = " ";
  } else if (String::len(%pad) > 1) {
    %pad = String::GetSubStr(%pad, 0, 1);
  }
  %result = %input;
  while (String::len(%result) < %length) {
    %result = strcat(%pad, %result);
  }
  return %result;
}

// util functions
function boldnum( %n ) {
    if (!String::Compare( %n, "-" ) || ( String::Compare( %n, "" ) && String::Compare( %n, "0" ) )) {
        return bold( %n );
    }
    else {
        return "0";
    }
	//return ( !String::Compare( %n, "-" ) || ( String::Compare( %n, "" ) &&  String::Compare( %n, "0" ) ) ) ? bold( %n ) : "0";
}

function alttag() {
	$Exporter::alttag = ( $Exporter::alttag + 1 ) & 1;
	return ( "alt" @ $Exporter::alttag );
}

function boldtime( %duration ) {
	if ( %duration == "-" )
		return ( %duration );
	else if ( %duration == "" || %duration == "0" ) 
		return ( "0" );
	%hr = String::lpad( floor( %duration / 3600 ), 2, "0" );
	%duration = ( %duration % 3600 );
	%min = String::lpad( floor( %duration / 60 ), 2, "0" ); 
	%duration = ( %duration % 60 );
	%sec = String::lpad( floor( %duration ), 2, "0" );
	%time = %min @ ":" @ %sec;
	if ( %hr > 0 )
		%time = %hr @ ":" @ %time;
	return boldnum( %time );
}

function xmlizename( %name ) {
	%name = String::Replace( %name, "&", "&amp;" );
	%name = String::Replace( %name, "<", "&lt;" );
	%name = String::Replace( %name, ">", "&gt;" );
	return ( %name );
}

function filenameizename( %name ) {
	%name = String::Replace( %name, "&", "_" );
	%name = String::Replace( %name, "<", "_" );
	%name = String::Replace( %name, ">", "_" );
	%name = String::Replace( %name, "|", "_" );
	%name = String::Replace( %name, "*", "_" );
	%name = String::Replace( %name, "$", "_" );
	%name = String::Replace( %name, ":", "_" );
	%name = String::Replace( %name, "?", "_" );
	%name = String::Replace( %name, "\\", "_" );
	%name = String::Replace( %name, "/", "_" );
	%name = String::Replace( %name, "@", "_" );
	%name = String::Replace( %name, "#", "_" );
	%name = String::Replace( %name, "%", "_" );
	%name = String::Replace( %name, "^", "_" );
	%name = String::Replace( %name, "~", "_" );
	return ( %name );
}

// %1 = title
$Exporter::header = "<!DOCTYPE\nhtml PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"\n \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n<html>\n<head>\n\t<title>%1</title>\n\t<link id=\"style\" rel=\"stylesheet\" type=\"text/css\" href=\"style.css\" />\n</head>\n";
$Exporter::footer = "</div>\n</body>\n</html>";

// %1 = date, %2 = map, %3 = length, %4 = score(0), %5 = score(1)
$Exporter::body = 
	"<body>\n" @
		div( "spacer" ) @
		div( "content",
			//span( "label", "Date: " ) @ span( "value", "%1" ) @ br() @
			span( "label", "Map: " ) @ span( "value", "%2" ) @ br() @
			span( "label", "Length: " ) @ span( "value", "%3" ) @ br() @
			span( "label", bold( "BE" ) @ " # Caps: " ) @ span( "value", "%4" ) @ br() @
			span( "label", bold( "DS" ) @ " # Caps: " ) @ span( "value", "%5" )
		) @
		"<div class=\"body\">\n";

$Exporter::timeheader =
	tr( "", "<th colspan=\"5\">Time Info</th>" ) @
	tr( "title",
		td( "t", "Name" ) @
		td( "c", "OBS" ) @
		td( "c", "BE" ) @	
		td( "c", "DS" ) @
		td( "c", "Flag" )
	);

$Exporter::timefooter = "</table>" @ div( "spacer" );

$Exporter::statsheader =
	tr( "", "<th colspan=\"18\">Match Stats (v3.3 Server)</th>" ) @
	tr( "title",
		td( "t", "Player" ) @
		td( "", " Rating " ) @
		td( "", " K/D " ) @
        td( "", " Suic " ) @
        td( "", " TKs " ) @
		td( "", " Disc " ) @
		td( "", " Nade " ) @
		td( "", " Chain " ) @
        td( "", " MAs " ) @
        td( "", " BBs " ) @
		td( "", " CKills " ) @
        td( "", " Ret " ) @
		td( "", " Grabs " ) @
		td( "", " Pckups " ) @
		td( "", " Drops " ) @
		td( "", " Assists " ) @
		td( "", " Caps " ) @
        td( "", " Flag Time " )
	);

$Exporter::funstatsheader =
	tr( "", "<th colspan=\"13\">Fun Stats</th>" ) @
	tr( "title",
		td( "t", "Player" ) @
		td( "", " Dmg Rating " ) @
        td( "", " Damage " ) @
        td( "", " Tm.Dmg " ) @
        td( "", " MA Nades " ) @
        td( "", " MA CKs " ) @
        td( "", " Cl.Ret " ) @
        td( "", " Int " ) @
        td( "", " E.Grabs " ) @
        td( "", " Catches " ) @
        td( "", " Clunks " ) @
        td( "", " Craters " ) @
        td( "", " NJs " )
	);

$Exporter::statsfooter = "</table>" @ div( "spacer" );

$Exporter::display["BE"] = "BE Totals";
$Exporter::display["DS"] = "DS Totals";
$Exporter::display["Home"] = "Enemy Flag Home";
$Exporter::display["Field"] = "Enemy Flag Field";

function Exporter::ExportTimeRow( %class, %team, %name ) {
	if ( String::len( %name ) > 16 )
		%dispname = $Exporter::display[ String::trim( %name ) ];
	else
		%dispname = xmlizename( %name );
	
    if ( (%name == $Marker::Home) || (%name == $Marker::Field) ) {

        html::emit(
            tr( %class,
                td( "t", %dispname ) @
                td( "c", boldtime( $Collector::timeplayer[ %name, -1 ] ) ) @ 
                td( "c", boldtime( $Collector::timeplayer[ %name,  0 ] ) ) @
                td( "c", boldtime( $Collector::timeplayer[ %name,  1 ] ) ) @
                td( "c", boldtime( $Collector::timeflag[ %team ^ 1, %name ] ) )
            )
        );
    }
    
    else {
        
        html::emit(
            tr( %class,
                td( "t", %dispname ) @
                td( "c", boldtime( $Collector::timeplayer[ %name, -1 ] ) ) @ 
                td( "c", boldtime( $Collector::timeplayer[ %name,  0 ] ) ) @
                td( "c", boldtime( $Collector::timeplayer[ %name,  1 ] ) ) @
                td( "c", boldtime( ($Collector::timeflag[ 0, %name ] + $Collector::timeflag[ 1, %name ]) ) )
            )
        );
    }
}

function Exporter::ExportTime() {
	//html::emit( "<table class=\"time\">\n" );
    html::emit( "<table class=\"stats\">\n" );
	html::emit( $Exporter::timeheader );

	for ( %team = 0; %team <= 1; %team++ ) {
		Stack::Reset( "teamlist" @ %team );

		for ( %i = 0; %i < Stack::Count( "teamlist" @ %team ); %i++ ) {
			%name = Stack::GetNext( "teamlist" @ %team );
			Exporter::ExportTimeRow( alttag(), %team, %name );
		}

		Exporter::ExportTimeRow( alttag(), %team, $Marker::Home );
		Exporter::ExportTimeRow( alttag(), %team, $Marker::Field );
        
		html::emit( tr( "l", "<td class=\"l\" colspan=\"5\"></td>" ) );
        
        if ($Exporter::Outcome == 0) {
            
            Exporter::ExportTimeRow( "total", %team, Marker::Team( %team ) );
            
        }
        else if ( ((%team == 0) && ($Exporter::Outcome == 1)) || ((%team == 1) && ($Exporter::Outcome == 2)) ) {
            
            Exporter::ExportTimeRow( "winner", %team, Marker::Team( %team ) );
            
        }
        else {
            
            Exporter::ExportTimeRow( "loser", %team, Marker::Team( %team ) );
        }

		html::emit( tr( "l", "<td class=\"l\" colspan=\"5\"></td>" ) );
	}
	
	html::emit( $Exporter::timefooter );
}

function Exporter::KillDeathPair( %name, %type ) {
	if ( %type == "" )
		return sprintf( "%1/%2", boldnum( $Collector::Kills[ %name ] ), boldnum( $Collector::Deaths[ %name ] ) );
    else if(%type == "MidAirDisc")
        return sprintf( "%1/%2", boldnum( $Collector::MAGiven[ %name ] ), boldnum( $Collector::MATaken[ %name ] ) );
    else if(%type == "BodyBlock")
        return sprintf( "%1/%2", boldnum( $Collector::BBGiven[ %name ] ), boldnum( $Collector::BBTaken[ %name ] ) );
    else if(%type == "DamageDealt")
        return sprintf( "%1/%2", boldnum( floor(($Collector::DamageOut[ %name ]/10)) ), boldnum( floor(($Collector::DamageIn[ %name ]/10)) ) );
    else if(%type == "TeamDamageDealt")
        return sprintf( "%1/%2", boldnum( floor(($Collector::TeamDamageOut[ %name ]/10)) ), boldnum( floor(($Collector::TeamDamageIn[ %name ]/10)) ) );
	else
		return sprintf( "%1/%2", boldnum( $Collector::Kills[ %name, %type ] ), boldnum( $Collector::Deaths[ %name, %type ] ) );
}

function Exporter::ExportStatsRow( %class, %name ) {
	//%dispname = ( String::len( %name ) > 16 ) ? String::trim( %name ) @ " Totals" : xmlizename( %name );
    
    if (String::len( %name ) > 16) {
       %dispname = String::trim( %name ) @ " Totals";
    }
    else {
       %dispname = xmlizename( %name );
    }
	
    if (%class == "total" || %class == "winner" || %class == "loser") {
        
        html::emit(
		tr( %class,
			td( "t", %dispname ) @
			td( "", boldnum( $Collector::Score[ %name ] ) ) @
			td( "", Exporter::KillDeathPair( %name ) ) @
            td( "", boldnum( $Collector::Suicides[ %name ] ) ) @
            td( "", sprintf( "%1/%2", boldnum( $Collector::TeamKills[ %name ] ), boldnum( $Collector::TeamDeaths[ %name ] ) ) ) @
            td( "", Exporter::KillDeathPair( %name, "Disc" ) ) @
            td( "", Exporter::KillDeathPair( %name, "Explosive" ) ) @
			td( "", Exporter::KillDeathPair( %name, "Chaingun" ) ) @
            td( "", Exporter::KillDeathPair( %name, "MidAirDisc" ) ) @
            td( "", Exporter::KillDeathPair( %name, "BodyBlock" ) ) @
			td( "", boldnum( $Collector::CarrierKills[ %name ] ) ) @
            td( "", boldnum( ($Collector::Returns[ %name ] + $Collector::StandoffReturns[ %name ]) ) ) @
			td( "", boldnum( $Collector::Grabs[ %name ] ) ) @
			td( "", boldnum( $Collector::Pickups[ %name ] ) ) @
			td( "", boldnum( $Collector::Drops[ %name ] ) ) @
			td( "", boldnum( $Collector::Assists[ %name ] ) ) @
			td( "cap", boldnum( $Collector::Caps[ %name ] ) ) @
            td( "", boldtime( $Collector::TotalFlagTime[ %name ] ) )
		)
        );
        
    }
    else {
        
        html::emit(
		tr( %class,
			td( "t", %dispname ) @
			td( "", boldnum( $Collector::Score[ %name ] ) ) @
			td( "", Exporter::KillDeathPair( %name ) ) @
            td( "", boldnum( $Collector::Suicides[ %name ] ) ) @
            td( "", sprintf( "%1/%2", boldnum( $Collector::TeamKills[ %name ] ), boldnum( $Collector::TeamDeaths[ %name ] ) ) ) @
            td( "", Exporter::KillDeathPair( %name, "Disc" ) ) @
            td( "", Exporter::KillDeathPair( %name, "Explosive" ) ) @
			td( "", Exporter::KillDeathPair( %name, "Chaingun" ) ) @
            td( "", Exporter::KillDeathPair( %name, "MidAirDisc" ) ) @
            td( "", Exporter::KillDeathPair( %name, "BodyBlock" ) ) @
			td( "", boldnum( $Collector::CarrierKills[ %name ] ) ) @
            td( "", boldnum( ($Collector::Returns[ %name ] + $Collector::StandoffReturns[ %name ]) ) ) @
			td( "", boldnum( $Collector::Grabs[ %name ] ) ) @
			td( "", boldnum( $Collector::Pickups[ %name ] ) ) @
			td( "", boldnum( $Collector::Drops[ %name ] ) ) @
			td( "", boldnum( $Collector::Assists[ %name ] ) ) @
			td( "", boldnum( $Collector::Caps[ %name ] ) ) @
            td( "", boldtime( $Collector::TotalFlagTime[ %name ] ) )
		)
        );
        
    }
}

function Exporter::ExportFunStatsRow( %class, %name ) {
	//%dispname = ( String::len( %name ) > 16 ) ? String::trim( %name ) @ " Totals" : xmlizename( %name );
    if (String::len( %name ) > 16) {
       %dispname = String::trim( %name ) @ " Totals";
    }
    else {
       %dispname = xmlizename( %name );
    }
    $Collector::DamageRating[ %name ] = ( floor($Collector::DamageOut[ %name ]/10) - floor($Collector::TeamDamageOut[ %name ]/10) );
	
	html::emit(
		tr( %class,
			td( "t", %dispname ) @
            td( "", boldnum( $Collector::DamageRating[ %name ] ) ) @
			td( "", Exporter::KillDeathPair( %name, "DamageDealt" ) ) @
			td( "", Exporter::KillDeathPair( %name, "TeamDamageDealt" ) ) @
            td( "", boldnum( $Collector::MANades[ %name ] ) ) @
            td( "", boldnum( $Collector::MidAirCK[ %name ] ) ) @
            td( "", boldnum( $Collector::ClutchReturns[ %name ] ) ) @
            td( "", boldnum( $Collector::Interceptions[ %name ]  ) ) @
            td( "", boldnum( $Collector::EGrabs[ %name ] ) ) @
            td( "", boldnum( $Collector::Catches[ %name ] ) ) @
            td( "", boldnum( $Collector::Clunks[ %name ] ) ) @
            td( "", boldnum( $Collector::Craters[ %name ] ) ) @
            td( "", boldnum( $Collector::NadeJumps[ %name ] ) )
		)
	);
}

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
    $Collector::TotalFlagTime[%teamname] = 0;
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
    $Collector::TotalFlagTime[%teamname] += $Collector::TotalFlagTime[%name];
}

function Exporter::FinalScore() {

    for ( %team = 0; %team <= 1; %team++ ) {

        %teamname = Marker::Team( %team + 2 );
        Exporter::ResetTotals(%teamname);
        Stack::Reset( "teamlist" @ %team );
        
        for ( %j = 0; %j < Stack::Count( "teamlist" @ %team ); %j++ ) {
            
            %name = Stack::GetNext( "teamlist" @ %team );
            Exporter::ActualTotals(%name, %teamname);
            
        }
    }

    %teamName0 = Marker::Team( 2 );
    %teamName1 = Marker::Team( 3 );
    
    //BE wins
    if ($Collector::Caps[%teamName0] > $Collector::Caps[%teamName1]) { return 1; }
    //DS wins
    else if ($Collector::Caps[%teamName0] < $Collector::Caps[%teamName1]) { return 2; }
    //tie game
    else { return 0; }
    
}

function Exporter::ExportStats() {
	html::emit( "<table class=\"stats\">\n" );
	html::emit( $Exporter::statsheader );
    
    $Exporter::Outcome = Exporter::FinalScore();

	for ( %team = 0; %team <= 1; %team++ ) {

        %teamname = Marker::Team( %team + 2 );
		Stack::Reset( "teamlist" @ %team );
        
		for ( %j = 0; %j < Stack::Count( "teamlist" @ %team ); %j++ ) {
            
            %name = Stack::GetNext( "teamlist" @ %team );
            Exporter::ExportStatsRow( alttag(), %name );
            
        }
        
        html::emit( tr( "", "<td class=\"l\" colspan=\"18\"></td>" ) );
        
        if ($Exporter::Outcome == 0) {
            
            Exporter::ExportStatsRow( "total", %teamname );
            
        }
        else if ( ((%team == 0) && ($Exporter::Outcome == 1)) || ((%team == 1) && ($Exporter::Outcome == 2)) ) {
            
            Exporter::ExportStatsRow( "winner", %teamname );
            
        }
        else {
            Exporter::ExportStatsRow( "loser", %teamname );
        }

        html::emit( tr( "", "<td class=\"l\" colspan=\"18\"></td>" ) );

	}

	html::emit( $Exporter::statsfooter );
}

function Exporter::ExportFunStats() {
	html::emit( "<table class=\"stats\">\n" );
	html::emit( $Exporter::funstatsheader );

	for ( %team = 0; %team <= 1; %team++ ) {
        
        %teamname = Marker::Team( %team + 2 );
		Stack::Reset( "teamlist" @ %team );
		for ( %j = 0; %j < Stack::Count( "teamlist" @ %team ); %j++ ) {
            
            %name = Stack::GetNext( "teamlist" @ %team );
            Exporter::ExportFunStatsRow( alttag(), %name );
             
        }
        
        html::emit( tr( "", "<td class=\"l\" colspan=\"13\"></td>" ) );
        
        if ($Exporter::Outcome == 0) {
            
            Exporter::ExportFunStatsRow( "total", %teamname );
            
        }
        else if ( ((%team == 0) && ($Exporter::Outcome == 1)) || ((%team == 1) && ($Exporter::Outcome == 2)) ) {
            
            Exporter::ExportFunStatsRow( "winner", %teamname );
            
        }
        else {
            Exporter::ExportFunStatsRow( "loser", %teamname );
        }
        
        html::emit( tr( "", "<td class=\"l\" colspan=\"13\"></td>" ) );
	}

	html::emit( $Exporter::statsfooter );
}

function Exporter::ExportMap( %name ) {
	echo( "EXPORTING " @ %name );
	
	%obj = newobject( "FileWriterDummy", FearGuiFormattedText, 0, 0, 0, 0 );
	flushExportText();

	%playername = filenameizename( %name );

	timestamp::array();
	%displaydate = sprintf( "%1-%2-%3 %4:%5:%6", $Time["yr"], $Time["mo"], $Time["dy"], $Time["hr"], $Time["mn"], $Time["sc"] );
	
	%suffix = "am";
	if ( $Time["hr"] > 12 ) {
		$Time["hr"] = String::lpad( $Time["hr"] - 12, 2, "0" );
		%suffix = "pm";
	}
	%filedate = sprintf( "%1-%2-%3_%4%5%6", $Time["yr"], $Time["mo"], $Time["dy"], $Time["hr"], $Time["mn"], %suffix );
	
	html::emitf( $Exporter::header, "Andrew's Map Stats" );
    
    html::emitf(
    $Exporter::body,
	%displaydate,
	$Collector::MissionName,
	boldtime( $Collector::Duration ),
    boldnum( $Collector::Caps[Marker::Team(0)] ),
	boldnum( $Collector::Caps[Marker::Team(1)] )
    );
    
	//Exporter::ExportTime();
	Exporter::ExportStats();
    Exporter::ExportFunStats();
	
	html::emit( $Exporter::footer );
    
    //random output will be 0-999
    %randomInt = floor(getRandom() * (1000 - 0.1));
    echo("GAMEID " @ %randomInt);
	
	exportObjectToScript( "FileWriterDummy", "temp\\collector" @ %randomInt @ ".html", true );
    //export( "$Collector*", "temp/collector" @ %randomInt @ ".cs" );
	deleteObject( nameToID("FileWriterDummy") );
	flushExportText();

}
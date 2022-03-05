$Marker::Player = "playerteam";
$Marker::Flag = "statflag";

function Collector::Clear() {
    
    $zadmin::Flag[0] = $Marker::Home;
    $zadmin::Flag[1] = $Marker::Home;
    $zadmin::FlagOffStand[0] = getSimTime();
    $zadmin::FlagOffStand[1] = getSimTime();
    
    deleteVariables( "$Collector*" );
    
    stack::clear( "playerlist" );
    TimeC::Clear( $Marker::Flag );
    TimeC::Clear( $Marker::Player );
}

function Collector::ResetAll(%name) {
    $Collector::Score[%name] = 0;
    $Collector::Kills[%name] = 0;
    $Collector::Deaths[%name] = 0;
    $Collector::Suicides[%name] = 0;
    $Collector::TeamKills[%name] = 0;
    $Collector::TeamDeaths[%name] = 0;
    $Collector::Kills[%name, "Disc"] = 0;
    $Collector::Deaths[%name, "Disc"] = 0;
    $Collector::Kills[%name, "Explosive"] = 0;
    $Collector::Deaths[%name, "Explosive"] = 0;
    $Collector::Kills[%name, "Chaingun"] = 0;
    $Collector::Deaths[%name, "Chaingun"] = 0;
    $Collector::CarrierKills[%name] = 0;
    $Collector::MidAirCK[%name] = 0;
    $Collector::Returns[%name] = 0;
    $Collector::StandoffReturns[%name] = 0;
    $Collector::Interceptions[%name] = 0;
    $Collector::BBGiven[%name] = 0;
    $Collector::BBTaken[%name] = 0;
    $Collector::Grabs[%name] = 0;
    $Collector::Pickups[%name] = 0;
    $Collector::Catches[%name] = 0;
    $Collector::Drops[%name] = 0;
    $Collector::Assists[%name] = 0;
    $Collector::Caps[%name] = 0;
    $Collector::MAGiven[%name] = 0;
    $Collector::MATaken[%name] = 0;
    $Collector::DamageOut[%name] = 0;
    $Collector::DamageIn[%name] = 0;
    $Collector::TeamDamageOut[%name] = 0;
    $Collector::TeamDamageIn[%name] = 0;
    $Collector::NadeJumps[%name] = 0;
    $Collector::MANades[%name] = 0;
    $Collector::Craters[%name] = 0;
    $Collector::ClutchReturns[%name] = 0;
    $Collector::EGrabs[%name] = 0;
    $Collector::Clunks[%name] = 0;
    $Collector::timeflag[ 0, %name ] = 0;
    $Collector::timeflag[ 1, %name ] = 0;
    $Collector::TotalFlagTime[%name] = 0;
}

function Collector::onStart() {

    Collector::Clear();
    
    $Collecting = true;
    $Collector::Manager = "ServerStats";
    $Collector::Start = getSimTime();
    $Collector::MissionName = $missionName;
    $Collector::TeamScore[0] = 0;
    $Collector::TeamScore[1] = 0;
    
    TimeC::Update( $Marker::Flag @ "0", $Marker::Home );
    TimeC::Update( $Marker::Flag @ "1", $Marker::Home );
    Collector::InitPlayers();
}

function Collector::onStop() {

    if(!$Collecting) { return; }
    
    $Collector::Stop = getSimTime();
    $Collector::Duration = ( $Collector::Stop - $Collector::Start );

    // finalize flag times
    TimeC::Update( $Marker::Flag @ "0", $Marker::Home );
    TimeC::Update( $Marker::Flag @ "1", $Marker::Home );

    Collector::ConstructFinalizeTeamLists();
    Exporter::ExportMap( $Collector::Manager );
    $Collecting = false;
}

function Collector::InitPlayers() {
    for ( %cl = Client::getFirst(); %cl != -1; %cl = Client::getNext( %cl ) ) {
        %name = Client::getName( %cl );
        %team = Client::getTeam( %cl );
        $Collector::PlayerTeam[ %name ] = %team;
        Collector::ResetAll(%name);
        TimeC::Update( $Marker::Player@%name, %team );
        Stack::PushUnique( "playerlist", %name );
    }
}

function Collector::ConstructFinalizeTeamLists() {
    Stack::Clear( "teamlist-1");
    Stack::Clear( "teamlist0" );
    Stack::Clear( "teamlist1" );

    deleteVariables( "$Collector::time*" );

    // get home/field times
    for ( %team = 0; %team <= 1; %team++ ) {
        $Collector::timeflag[%team, $Marker::Home] = TimeC::Duration( $Marker::Flag @ ( %team ), $Marker::Home );
        $Collector::timeflag[%team, $Marker::Field] = TimeC::Duration( $Marker::Flag @ ( %team ), $Marker::Field );
    }

    // finalize all the player team/flag times
    Stack::Reset( "playerlist" );
    for ( %i = 0; %i < stack::count( "playerlist" ); %i++ ) {
        %name = Stack::GetNext( "playerlist" );
        
        if($Collector::Score[ %name ] == 0) {
            continue;
        }

        // finalize the team time
        TimeC::Update( $Marker::Player@%name, -1 );

        //echo( %name );
        for ( %team = -1; %team <= 1; %team++ ) {
            $Collector::timeplayer[%name, %team] = TimeC::Duration( $Marker::Player @ %name, %team );
            //echo( $Collector::timeplayer[%name, %team] );
        }
        
        // Place player on the team they join in the 1st half
        
        if ($Collector::PlayerTeam[%name] == 0) {
            %team = 0;
        }
        else if ($Collector::PlayerTeam[%name] == 1) {
            %team = 1;
        }
        else {
            %team = -1;
        }
        
        $Collector::timeflag[ 0, %name ] = TimeC::Duration( $Marker::Flag @ ( 0 ), %name );
        $Collector::timeflag[ 1, %name ] = TimeC::Duration( $Marker::Flag @ ( 1 ), %name );
        $Collector::TotalFlagTime[ %name ] = ($Collector::timeflag[ 0, %name ] + $Collector::timeflag[ 1, %name ]);

        // add player time to team totals
        Stack::Push( "teamlist" @ %team, %name );
        for ( %j = 0; %j <= 1; %j++ ) {
            $Collector::timeplayer[$Marker::Home, %j] = "-";
            $Collector::timeplayer[$Marker::Field, %j] = "-";
            $Collector::timeplayer[Marker::Team(%team), %j] += $Collector::timeplayer[%name, %j];
            $Collector::timeflag[%j,Marker::Team(%team)] += TimeC::Duration( $Marker::Flag @ ( %j ), %name );
        }
    }
}

function Collector::onPlayerJoin( %cl ) {
    %name = Client::getName( %cl );
    %team = Client::getTeam( %cl );
    
    $Collector::Score[ %name ] += 0;
    if( ($Collector::PlayerTeam[%name] != 0) && ($Collector::PlayerTeam[%name] != 1) ) {
        
        $Collector::PlayerTeam[%name] = -1;
    }
    
    TimeC::Update( $Marker::Player @ %name, %team );
    Stack::PushUnique( "playerlist", %name );
}

function Collector::onPlayerDrop( %cl ) {
    TimeC::Update( $Marker::Player@Client::getName( %cl ), -2 );
}

function Collector::onPlayerChange( %cl, %newteam ) {
    
    %name = Client::getName( %cl );
    
    if( ($Collector::PlayerTeam[%name] != 0) && ($Collector::PlayerTeam[%name] != 1) ) {
        
        $Collector::PlayerTeam[%name] = %newteam;
        
    }
    
    TimeC::Update( $Marker::Player @ %name, %newteam );
}

function Collector::onClientScoreAdd( %cl, %scoreAdd ) {
    %name = Client::getName( %cl );
    $Collector::Score[%name] += %scoreAdd;
    $Collector::Score[ Marker::Team( Client::getTeam( %cl ) ) ] += %scoreAdd;
}

function Collector::onFlagGrab( %team, %cl ) {
    %name = Client::getName( %cl );
    TimeC::Update( $Marker::Flag @ %team, %name );
    $Collector::Grabs[%name]++;
    $Collector::Grabs[Marker::Team( %team ^ 1 )]++;
}

function Collector::onFlagEGrab( %cl, %flagTeam ) {
    %name = Client::getName( %cl );
    %team = Client::getTeam( %cl );

    $Collector::EGrabs[ %name ]++;
    $Collector::EGrabs[ Marker::Team( %team ) ]++;

}

function Collector::onFlagPickup( %team, %cl ) {
    %name = Client::getName( %cl );
    TimeC::Update( $Marker::Flag @ %team, %name );
    $Collector::Pickups[%name]++;
    $Collector::Pickups[Marker::Team( %team ^ 1 )]++;
}

function Collector::onFlagCatch( %team, %cl ) {
    %name = Client::getName( %cl );
    
    $Collector::Catches[%name]++;
    $Collector::Catches[Marker::Team( %team ^ 1 )]++;
}

function Collector::onFlagReturn( %team, %cl ) {
    TimeC::Update( $Marker::Flag @ %team, $Marker::Home );
    if ( !%cl )
        return;

    %name = Client::getName( %cl );
    $Collector::Returns[%name]++;
    $Collector::Returns[Marker::Team( %team )]++;
}

function Collector::onFlagClutchReturn( %cl, %flagTeam ) {
    %name = Client::getName( %cl );
    %team = Client::getTeam( %cl );

    $Collector::ClutchReturns[ %name ]++;
    $Collector::ClutchReturns[ Marker::Team( %team ) ]++;
}

function Collector::onFlagInt( %team, %cl ) {
    if ( !%cl )
        return;

    %name = Client::getName( %cl );
    $Collector::Interceptions[%name]++;
    $Collector::Interceptions[Marker::Team( %team )]++;
}

function Collector::onFlagStandoffReturn( %team, %cl ) {
    if ( !%cl )
        return;

    %name = Client::getName( %cl );
    $Collector::StandoffReturns[%name]++;
    $Collector::StandoffReturns[Marker::Team( %team )]++;
}

function Collector::onFlagCap( %team, %cl ) {
    %name = Client::getName( %cl );

    TimeC::Update( $Marker::Flag @ %team, $Marker::Home );

    $Collector::Caps[%name]++;
    $Collector::Caps[Marker::Team( %team ^ 1 )] += 1;
}


function Collector::onFlagAssist( %cl ) {
    %name = Client::getName( %cl );
    %team = Client::getTeam( %cl ) ^ 1;
    $Collector::Assists[%name]++;
    $Collector::Assists[Marker::Team( %team ^ 1 )] += 1;
}

function Collector::onFlagDrop( %team, %cl ) {
    %name = Client::getName( %cl );
    
    TimeC::Update( $Marker::Flag @ %team, $Marker::Field );

    $Collector::Drops[%name]++;
    $Collector::Drops[Marker::Team( %team ^ 1 )]++;
}

function Collector::onFlagCarrierKill( %killer ) {
    %killerName = Client::getName( %killer );
    %killerTeam = Client::getTeam( %killer );
    $Collector::CarrierKills[ %killerName ]++;
    $Collector::CarrierKills[ Marker::Team( %killerTeam ) ]++;
}

function Collector::onMidAirCK( %shooter ) {
    
    %killerTeam = Client::getTeam( %shooter );
    %killerName = Client::getName( %shooter );
    
    $Collector::MidAirCK[ %killerName ]++;
    $Collector::MidAirCK[ Marker::Team( %killerTeam ) ]++;
    
}

function Collector::onNadeJump( %cl, %speed ) {
    
    %name = Client::getName( %cl );
    %team = Client::getTeam( %cl );
    
    $Collector::NadeJumps[ %name ]++; 
    $Collector::NadeJumps[Marker::Team( %team )]++;
}

function Collector::onMidAirDisc( %shooter, %victim ) {
    %shooterTeam = Client::getTeam( %shooter );
    %victimTeam = Client::getTeam( %victim );
    %shooterName = Client::getName( %shooter );
    %victimName = Client::getName( %victim );

    $Collector::MAGiven[ %shooterName ]++;
    $Collector::MATaken[ %victimName ]++;
    
    $Collector::MAGiven[Marker::Team( %shooterTeam )]++;
    $Collector::MATaken[Marker::Team( %victimTeam )]++;
}

function Collector::onMidAirNade( %shooter ) {
    
    %shooterTeam = Client::getTeam( %shooter );
    %shooterName = Client::getName( %shooter );
    
    $Collector::MANades[ %shooterName ]++;
    $Collector::MANades[ Marker::Team( %shooterTeam ) ]++;

}

function Collector::onBodyBlock( %shooter, %victim ) {
    %shooterTeam = Client::getTeam( %shooter );
    %victimTeam = Client::getTeam( %victim );
    %shooterName = Client::getName( %shooter );
    %victimName = Client::getName( %victim );

    $Collector::BBGiven[ %shooterName ]++;
    $Collector::BBTaken[ %victimName ]++;
    
    $Collector::BBGiven[Marker::Team( %shooterTeam )]++;
    $Collector::BBTaken[Marker::Team( %victimTeam )]++;
}

function Collector::onDamageDealt( %shooter, %victim, %damage ) {
    
    %shooterTeam = Client::getTeam( %shooter );
    %victimTeam = Client::getTeam( %victim );
    %shooterName = Client::getName( %shooter );
    %victimName = Client::getName( %victim );
    
    $Collector::DamageOut[ %shooterName ] += %damage;
    $Collector::DamageIn[ %victimName ] += %damage;
    
    $Collector::DamageOut[Marker::Team( %shooterTeam )] += %damage;
    $Collector::DamageIn[Marker::Team( %victimTeam )] += %damage;
    
}

function Collector::onTeamDamageDealt( %shooter, %victim, %damage ) {
    
    %shooterTeam = Client::getTeam( %shooter );
    %victimTeam = Client::getTeam( %victim );
    %shooterName = Client::getName( %shooter );
    %victimName = Client::getName( %victim );

    $Collector::TeamDamageOut[ %shooterName ] += %damage;
    $Collector::TeamDamageIn[ %victimName ] += %damage;
    
    $Collector::TeamDamageOut[Marker::Team( %shooterTeam )] += %damage;
    $Collector::TeamDamageIn[Marker::Team( %victimTeam )] += %damage;
    
}

function Collector::onClientKilled( %killer, %victim, %damageType ) {
    %killerteam = Client::getTeam( %killer );
    %victimteam = Client::getTeam( %victim );
    %killer = Client::getName( %killer );
    %victim = Client::getName( %victim );

    $Collector::Kills[ %killer ]++;
    $Collector::Kills[ %killer, %damageType ]++;
    $Collector::Kills[ %killer, %victim ]++;
    $Collector::Kills[ Marker::Team( %killerteam ) ]++;
    $Collector::Kills[ Marker::Team( %killerteam ), %damageType ]++;
    $Collector::Deaths[ %victim ]++;
    $Collector::Deaths[ %victim, %damageType ]++;
    $Collector::Deaths[ Marker::Team( %victimteam ) ]++;
    $Collector::Deaths[ Marker::Team( %victimteam ), %damageType ]++;
}

function Collector::onClientTeamKilled( %killer, %victim, %damageType ) {
    %killerteam = Client::getTeam( %killer );
    %victimteam = Client::getTeam( %victim );
    %killer = Client::getName( %killer );
    %victim = Client::getName( %victim );
    
    $Collector::TeamKills[ %killer ]++;
    $Collector::TeamKills[ Marker::Team( %killerteam ) ]++;
    $Collector::TeamDeaths[ %victim ]++;
    $Collector::TeamDeaths[ Marker::Team( %victimteam ) ]++;
}

function Collector::onClientSuicided( %victim, %damageType ) {
    %victimTeam = Client::getTeam( %victim );
    %victimName = Client::getName( %victim );

    $Collector::Suicides[ %victimName ]++;
    $Collector::Suicides[ Marker::Team( %victimTeam ) ]++;
    if(%damageType == "Landing"){
       $Collector::Craters[ %victimName ]++;
       $Collector::Craters[ Marker::Team( %victimTeam ) ]++;
    }
}

function Collector::onPlayerClunk( %cl ) {
    
    %clTeam = Client::getTeam( %cl );
    %clName = Client::getName( %cl );
    
    $Collector::Clunks[ %clName ]++;
    $Collector::Clunks[ Marker::Team( %clTeam ) ]++;

}

$Collecting = false;
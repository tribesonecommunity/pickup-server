$zadmin::WeaponConversion[ "Laser Rifle" ] = "Laser";
$zadmin::WeaponConversion[ "Chaingun" ] = "Chaingun";
$zadmin::WeaponConversion[ "Disc Launcher" ] = "Disc";
$zadmin::WeaponConversion[ "Explosives" ] = "Explosive";
$zadmin::WeaponConversion[ "Elf Gun" ] = "ELF";
$zadmin::WeaponConversion[ "Vehicle" ] = "Impact";

function zadmin::getWeapon( %weapon ) {
    %conv = $zadmin::WeaponConversion[ %weapon ];
    
    if (%conv != "") {
        return %conv;
    }
    return %weapon;
}

function Stats::FlagTaken( %teamid, %cl ) {
    $zadmin::lastName[ %cl ] = Client::getName( %cl );
    L::PushUnique( "zadminflag" @ %teamid, %cl );
    if ( $zadmin::Flag[%teamid] == $Marker::Home ) {
        $zadmin::FlagOffStand[%teamid] = getSimTime();
        OldRatings::scoreEvent( %cl, "Grab" );
        Collector::onFlagGrab( %teamid, %cl );
    } else {
        OldRatings::scoreEvent( %cl, "Pickup" );
        Collector::onFlagPickup( %teamid, %cl );
    }
    $zadmin::Flag[%teamid] = %cl;   
}

function Stats::FlagDropped( %teamid, %cl ) {
    $Collector::FlagDropTime[ %cl ] = getSimTime();
    OldRatings::scoreEvent( %cl, "Drop" );
    $zadmin::Flag[%teamid] = $Marker::Field;
    Collector::onFlagDrop( %teamid, %cl );
}

function Stats::FlagReturned( %teamid, %cl ) {
    if ( %cl ) {
        if ( ( getSimTime() - $zadmin::FlagOffStand[%teamid] ) > 90 ) {
            OldRatings::scoreEvent( %cl, "StandoffReturn" );
            Collector::onFlagStandoffReturn( %teamid, %cl );
        }
        OldRatings::scoreEvent( %cl, "Return" );
    }
    $zadmin::Flag[%teamid] = $Marker::Home;
    L::Clear( "zadminflag" @ %teamid );
    Collector::onFlagReturn( %teamid, %cl );
}

function Stats::FlagCaptured( %teamid, %cl ) {
    $zadmin::Flag[%teamid] = $Marker::Home;
    $zadmin::FlagOffStand[%teamid] = getSimTime();
    %tag = "zadminflag" @ %teamid;
    %count = L::Reset( %tag );
    for ( %i = 0; %i < %count; %i++ ) {
        %assistercl = L::GetNext( %tag );
        if ( Client::getName( %assistercl ) != $zadmin::lastName[ %assistercl ] )
            continue;
        if ( %assistercl != %cl ) {
            OldRatings::scoreEvent( %assistercl, "Assist" );
            Collector::onFlagAssist( %assistercl );
        }
    }
    L::Clear( %tag );
    OldRatings::scoreEvent( %cl, "Cap" );
    Collector::onFlagCap( %teamid, %cl );
}

function Stats::KillTrak( %killer, %victim, %weapon ) {
    %weapon = zadmin::getWeapon( %weapon );
    %victimteam = Client::getTeam( %victim );
    %killerteam = Client::getTeam( %killer );
    
    if ( !%killer ) {
        //do nothing
    }
    else if ( %killer == %victim ) {
        OldRatings::scoreEvent( %killer, "Suicide" );
        Collector::onClientSuicided( %victim, %weapon );
        
    }
    else if ( %victimteam == %killerteam ) {
        OldRatings::scoreEvent( %killer, "TeamKill" );
        Collector::onClientTeamKilled( %killer, %victim, %weapon );
    }
    else {
        
        OldRatings::scoreEvent( %killer, %weapon @ "Kill" );
        OldRatings::scoreEvent( %victim, %weapon @ "Death" );
        Collector::onClientKilled( %killer, %victim, %weapon );
        
    }
    
    if ( %killer && %victim && ( %killer != %victim ) ) {
        
        $Collector::PlayerDeadTime[ %victim ] = getSimTime();

        if ( ( $Collector::PlayerDeadTime[ %victim ] - $Collector::FlagDropTime[ %victim ] ) < 1.5 ) {
            
            OldRatings::scoreEvent( %killer, "CarrierKill" );
            Collector::onFlagCarrierKill( %killer );

            //Mid-air CK - Disc
            if ( ( $Collector::PlayerDeadTime[ %victim ] - $Collector::PlayerMATime[ %victim ] ) < 1.5 ) {
                
                Collector::onMidAirCK( %killer );
                
            }
            //Mid-air CK - Nade
            else if ( ( $Collector::PlayerDeadTime[ %victim ] - $Collector::PlayerMANadeTime[ %victim ] ) < 1.5 ) {
                
                Collector::onMidAirCK( %killer );
                
            }
            else {}
        }
    }
}

function Stats::MidAirDisc( %shooter, %victim, %time ) {
    
    $Collector::PlayerMATime[ %victim ] = getSimTime();
    Collector::onMidAirDisc( %shooter, %victim );
}

function Stats::MidAirNade( %shooter, %victim ) {
    
    $Collector::PlayerMANadeTime[ %victim ] = getSimTime();
    Collector::onMidAirNade( %shooter );
}
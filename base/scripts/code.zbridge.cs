//Event::Attach( eventConnectionAccepted, zadmin::onConnectionAccepted );

//function Stats::ITXT( %msg ) { if ( %sv == 2048 ) Control::setValue(EnergyDisplayText, %msg); }

//function Stats::ItemReceived( %cl, %item, %count) { Event::Trigger(EventItemReceived, %item, %count); }

$zadmin::WeaponConversion[ "Laser Rifle" ] = "Laser";
$zadmin::WeaponConversion[ "Chaingun" ] = "Chaingun";
$zadmin::WeaponConversion[ "Disc Launcher" ] = "Disc";
$zadmin::WeaponConversion[ "Explosives" ] = "Explosive";
$zadmin::WeaponConversion[ "Elf Gun" ] = "ELF";
$zadmin::WeaponConversion[ "Vehicle" ] = "Impact";

function Stats::zAdminActiveMode( %state ) {
	if ( %sv != 2048 )
		return;
	onClientMessageLegacy.detach();
	//Event::Detach( eventServerMessage, KillTrak::Parse );
}

function zadmin::onConnectionAccepted() {
	if ( getNetcodeVersion() <= 1 ) {
		//echoc( $CON_GREEN, $Server::Address );
		if ( ( String::FindSubStr($Server::Address, "LOOPBACK") == -1 ) || $PlayingDemo ) {
			onClientMessageLegacy.attach( onClientMessage, before );
			//Event::Attach( eventServerMessage, KillTrak::Parse );
		}
	} else {
		onClientMessageLegacy.detach();
		//Event::Detach( eventServerMessage, KillTrak::Parse );
	}
	
	remoteEval(2048, zAdminActiveMode);
	zadmin::reset();
}

function zadmin::reset() {
	$zadmin::Flag[0] = $Marker::Home;
	$zadmin::Flag[1] = $Marker::Home;
	$zadmin::FlagOffStand[0] = getSimTime();
	$zadmin::FlagOffStand[1] = getSimTime();
	deleteVariables( "$zadmin::FlagDropTime*" );
}

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
		//Event::Trigger( eventFlagGrab, %teamid, %cl );
        Collector::onFlagGrab( %teamid, %cl );
	} else {
		OldRatings::scoreEvent( %cl, "Pickup" );
		//Event::Trigger( eventFlagPickup, %teamid, %cl );
        Collector::onFlagPickup( %teamid, %cl );
	}
	$zadmin::Flag[%teamid] = %cl;	
	//Event::Trigger( eventFlagUpdate, %cl );
}

function Stats::FlagDropped( %teamid, %cl ) {
	$zadmin::FlagDropTime[ %cl ] = getSimTime();
	OldRatings::scoreEvent( %cl, "Drop" );
	$zadmin::Flag[%teamid] = $Marker::Field;
	//Event::Trigger( eventFlagDrop, %teamid, %cl );
	//Event::Trigger( eventFlagUpdate, %cl );
    Collector::onFlagDrop( %teamid, %cl );
}

function Stats::FlagReturned( %teamid, %cl ) {
	if ( %cl ) {
		if ( ( getSimTime() - $zadmin::FlagOffStand[%teamid] ) > 90 ) {
			OldRatings::scoreEvent( %cl, "StandoffReturn" );
			//Event::Trigger( eventFlagStandoffReturn, %teamid, %cl );
            Collector::onFlagStandoffReturn( %teamid, %cl );
		}
		OldRatings::scoreEvent( %cl, "Return" );
	}
	$zadmin::Flag[%teamid] = $Marker::Home;
	L::Clear( "zadminflag" @ %teamid );
	//Event::Trigger( eventFlagReturn, %teamid, %cl );
	//Event::Trigger( eventFlagUpdate, %cl );
    Collector::onFlagReturn( %teamid, %cl );
}

function Stats::FlagCaptured( %teamid, %cl ) {
	$zadmin::Flag[%teamid] = $Marker::Home;
	$zadmin::FlagOffStand[%teamid] = getSimTime();
	%tag = "zadminflag" @ %teamid;
	%count = L::Reset( %tag );
	%name = Client::getName( %cl );
	for ( %i = 0; %i < %count; %i++ ) {
		%assistercl = L::GetNext( %tag );
		if ( Client::getName( %assistercl ) != $zadmin::lastName[ %assistercl ] )
			continue;
		if ( %assistercl != %cl ) {
			OldRatings::scoreEvent( %assistercl, "Assist" );
			//Event::Trigger( eventFlagAssist, %assistercl );
            Collector::onFlagAssist( %cl );
		}
	}
	L::Clear( %tag );
	OldRatings::scoreEvent( %cl, "Cap" );
	//Event::Trigger(eventFlagCap, %teamid, %cl);
	//Event::Trigger( eventFlagUpdate, %cl );
    Collector::onFlagCap( %teamid, %cl );
}

function Stats::KillTrak( %killer, %victim, %weapon ) {
	%weapon = zadmin::getWeapon( %weapon );
	%victimteam = Client::getTeam( %victim );
	%killerteam = Client::getTeam( %killer );
    
	if ( !%killer ) {
		//Event::Trigger( eventClientDied, %victim, %weapon );
    }
	else if ( %killer == %victim ) {
		OldRatings::scoreEvent( %killer, "Suicide" );
		//Event::Trigger( eventClientSuicided, %killer, %weapon );
        Collector::onClientSuicided( %victim, %weapon );
        
	} else if ( %victimteam == %killerteam ) {
		OldRatings::scoreEvent( %killer, "TeamKill" );
		//Event::Trigger( eventClientTeamKilled, %killer, %victim, %weapon );
        Collector::onClientTeamKilled( %killer, %victim, %weapon );
        
	} else {
		OldRatings::scoreEvent( %killer, %weapon @ "Kill" );
		OldRatings::scoreEvent( %victim, %weapon @ "Death" );
        
		//Event::Trigger( eventClientKilled, %killer, %victim, %weapon );
        Collector::onClientKilled( %killer, %victim, %weapon );
        
	}
	
	if ( %killer && %victim && ( %killer != %victim ) ) {
        
        $zadmin::PlayerDeadTime[ %victim ] = getSimTime();
        
		if ( ( $zadmin::PlayerDeadTime[ %victim ] - $zadmin::FlagDropTime[ %victim ] ) < 1.5 ) {
			OldRatings::scoreEvent( %killer, "CarrierKill" );
			//Event::Trigger( eventFlagCarrierKill, %killer );
            Collector::onFlagCarrierKill( %killer );
            
            //Mid-air CK
            if ( ( $zadmin::PlayerDeadTime[ %victim ] - $zadmin::PlayerMATime[ %victim ] )  < 1.5) {
                //Event::Trigger( eventMidAirCK, %killer );
                Collector::onMidAirCK( %killer );
            }
            else if ( ( $zadmin::PlayerDeadTime[ %victim ] - $zadmin::PlayerMANadeTime[ %victim ] )  < 1.5) {
                //Event::Trigger( eventMidAirCK, %killer );
                Collector::onMidAirCK( %killer );
            }
            else {}
		}
	}
}

function Stats::MidAirDisc( %shooter, %victim, %time ) {
    
    $zadmin::PlayerMATime[ %victim ] = getSimTime();
	//Event::Trigger( eventMidAirDisc, %shooter, %victim );
    
    Collector::onMidAirDisc( %shooter, %victim );
}

function Stats::MidAirNade( %shooter, %victim ) {
    
    $zadmin::PlayerMANadeTime[ %victim ] = getSimTime();
	//Event::Trigger( eventMidAirNade, %shooter );
    Collector::onMidAirNade( %shooter );
}
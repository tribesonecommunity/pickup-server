$OldRatings[ "ChaingunKill" ] = 25;
$OldRatings[ "PlasmaKill" ] = 15;
$OldRatings[ "DiscKill" ] = 25;
$OldRatings[ "ExplosiveKill" ] = 25;
$OldRatings[ "LaserKill" ] = 20;
$OldRatings[ "MortarKill" ] = 15;
$OldRatings[ "BlasterKill" ] = 5;
$OldRatings[ "ElfKill" ] = 25;
$OldRatings[ "ImpactKill" ] = 25;
$OldRatings[ "Suicide" ] = -5;
$OldRatings[ "TeamKill" ] = -5;
$OldRatings[ "CarrierKill" ] = 45;
$OldRatings[ "Grab" ] = 10;
$OldRatings[ "Pickup" ] = 10;
$OldRatings[ "Drops" ] = -5;
$OldRatings[ "Return" ] = 10;
$OldRatings[ "StandoffReturn" ] = 25;
$OldRatings[ "Assist" ] = 125;
$OldRatings[ "Cap" ] = 175;
$OldRatings[ "Death" ] = -2;
$OldRatings[ "MortarDeath" ] = -1;

$Stats::Rating["Unknown"] = 0;
$Stats::Rating["Teamkill"] = $OldRatings[ "TeamKill" ];
$Stats::Rating["MineTeamkill"] = 0;
$Stats::Rating["Suicide"] = $OldRatings[ "Suicide" ];
$Stats::Rating["Vehicle"] = $OldRatings[ "ImpactKill" ];
$Stats::Rating["Chaingun"] = $OldRatings[ "ChaingunKill" ];
$Stats::Rating["Turret"] = $OldRatings[ "Death" ];
$Stats::Rating["Plasma"] = $OldRatings[ "PlasmaKill" ];
$Stats::Rating["Disc Launcher"] = $OldRatings[ "DiscKill" ];
$Stats::Rating["Explosives"] = $OldRatings[ "ExplosiveKill" ];
$Stats::Rating["Laser Rifle"] = $OldRatings[ "LaserKill" ];
$Stats::Rating["Mortar"] = $OldRatings[ "MortarKill" ];
$Stats::Rating["Blaster"] = $OldRatings[ "BlasterKill" ];
$Stats::Rating["Elf Gun"] = $OldRatings[ "ElfKill" ];
$Stats::Rating["Crushed"] = 0;
$Stats::Rating["Explosion"] = 0;
$Stats::Rating["Missile"] = 0;
$Stats::Rating["Grab"] = $OldRatings[ "Grab" ];
$Stats::Rating["Pickup"] = $OldRatings[ "Pickup" ];
$Stats::Rating["Drop"] = $OldRatings[ "Drops" ];
$Stats::Rating["CarrierKill"] = $OldRatings[ "CarrierKill" ];
$Stats::Rating["Return"] = $OldRatings[ "Return" ];
$Stats::Rating["StandoffReturn"] = $OldRatings[ "StandoffReturn" ];
$Stats::Rating["Assist"] = $OldRatings[ "Assist" ];
$Stats::Rating["Cap"] = $OldRatings[ "Cap" ];
$Stats::Rating["MortarDeath"] = $OldRatings[ "MortarDeath" ];
$Stats::Rating["OtherDeath"] = $OldRatings[ "Death" ];
$Stats::Rating["NadeJump"] = 0;
$Stats::Rating["MidAir"] = 0;
$Stats::Rating["MidAirLong"] = 0;
$Stats::Rating["MidAirLongCatch"] = 0;

function OldRatings::scoreEvent( %cl, %event ) {
	if ( String::findSubStr( %event, "Death" ) != -1 ) {
		if ( %event != "MortarDeath" )
			%event = "Death";
	}
	%value = $OldRatings[ %event ];
	if ( %value == "" )
		%value = 0;

    Collector::onClientScoreAdd( %cl, %value );
    ClientEvents::onClientScoreChange( %cl, %value, 1 );
}

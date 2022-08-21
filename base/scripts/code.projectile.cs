//----------------------------------------------------------------------------

$ImpactDamageType      = -1;
$LandingDamageType     =  0;
$BulletDamageType      =  1;
$EnergyDamageType      =  2;
$PlasmaDamageType      =  3;
$ExplosionDamageType   =  4;
$ShrapnelDamageType    =  5;
$LaserDamageType       =  6;
$MortarDamageType      =  7;
$BlasterDamageType     =  8;
$ElectricityDamageType =  9;
$CrushDamageType       = 10;
$DebrisDamageType      = 11;
$MissileDamageType     = 12;
$MineDamageType        = 13;

//----------------------------------------------------------------------------

function Lightning::damageTarget(%target, %timeSlice, %damPerSec, %enDrainPerSec, %pos, %vec, %mom, %shooterId)
{
   %damVal = %timeSlice * %damPerSec;
   %enVal  = %timeSlice * %enDrainPerSec;

   GameBase::applyDamage(%target, $ElectricityDamageType, %damVal, %pos, %vec, %mom, %shooterId);

   %energy = GameBase::getEnergy(%target);
   %energy = %energy - %enVal;
   if (%energy < 0) {
      %energy = 0;
   }
   GameBase::setEnergy(%target, %energy);
}

//----------------------------------------------------------------------------

function RepairBolt::onAcquire(%this, %player, %target)
{
	%client = Player::getClient(%player);

	if (%target == %player)
	{
		%player.repairTarget = -1;
		if (GameBase::getDamageLevel(%player) != 0)
		{
			%player.repairRate = 0.05;
			%player.repairTarget = %player;
			Client::sendMessage(%client, 0, "AutoRepair On");
		}
		else
		{
			Client::sendMessage(%client,0,"Nothing in range");
			Player::trigger(%player, $WeaponSlot, false);
			return;
		}
	}
	else
	{
		%player.repairTarget = %target;
		%player.repairRate   = 0.1;

		if (getObjectType(%player.repairTarget) == "Player")
		{
			%rclient = Player::getClient(%player.repairTarget);
			%name = Client::getName(%rclient);
		}
		else
		{ 
			%name = GameBase::getMapName(%target);
			%datablock = GameBase::getDataName(%player.repairTarget);
			
			if(%name == "")
			{
				%name = (GameBase::getDataName(%player.repairTarget)).description;
			}
			
			//keep idiots from making pubs worse by repairing rocket/motion turrets
			if (!$Server::TourneyMode && $zadmin::pref::antirape::norepair)
			{
				if ( (%datablock == "IndoorTurret") || (%datablock == "RocketTurret") )
				{
					Client::sendMessage(%client,1,"There are more productive things for you to be doing~wLeftMissionArea.wav");
					Player::trigger(%player,$WeaponSlot,false);
					%player.repairTarget = -1;
					return;
				}
			}
		}
		if (GameBase::getDamageLevel(%player.repairTarget) == 0)
		{
			Client::sendMessage(%client,0,%name @ " is not damaged");
			Player::trigger(%player,$WeaponSlot,false);
			%player.repairTarget = -1;
			return;
		}
		if (getObjectType(%player.repairTarget) == "Player")
		{
			Client::sendMessage(%rclient,0,"Being repaired by " @ Client::getName(%client));
		}
		Client::sendMessage(%client,0,"Repairing " @ %name);
	}

	%rate = GameBase::getAutoRepairRate(%player.repairTarget) + %player.repairRate;
	GameBase::setAutoRepairRate(%player.repairTarget,%rate);
}

//----------------------------------------------------------------------------

function RepairBolt::onRelease(%this, %player)
{
	%object = %player.repairTarget;
	if (%object != -1) {
		%client = Player::getClient(%player);
		if (%object == %player) {
			Client::sendMessage(%client,0,"AutoRepair Off");
		}
		else {
			if (GameBase::getDamageLevel(%object) == 0) {
				Client::sendMessage(%client,0,"Repair Done");
			}
			else {
				Client::sendMessage(%client,0,"Repair Stopped");
			}
		}
		%rate = GameBase::getAutoRepairRate(%object) - %player.repairRate;
      if (%rate < 0)
         %rate = 0;
      
		GameBase::setAutoRepairRate(%object,%rate);
	}
}

//----------------------------------------------------------------------------

function RepairBolt::checkDone(%this, %player)
{
	if (Player::isTriggered(%player,$WeaponSlot) && 
       Player::getMountedItem(%player,$WeaponSlot) == RepairGun &&
		 %player.repairTarget != -1) {
		%object = %player.repairTarget;
		if (%object == %player) {
			if (GameBase::getDamageLevel(%player) == 0) {
				Player::trigger(%player,$WeaponSlot,false);
				return;
			}
		}
		else {
			if (GameBase::getDamageLevel(%object) == 0) {
				Player::trigger(%player,$WeaponSlot,false);
				return;
			}
		}
	}
}
//----------------------------------------------------------------------------

$ItemFavoritesKey = "";  // Change this if you add new items
                         // and don't want to mess up everyone's
                         // favorites - just put in something
                         // that uniquely describes your new stuff.

//----------------------------------------------------------------------------

$ItemPopTime = 30;

$ToolSlot=0;
$WeaponSlot=0;
$BackpackSlot=1;
$FlagSlot=2;
$DefaultSlot=3;

$AutoUse[Blaster] = True;
$AutoUse[Chaingun] = True;
$AutoUse[PlasmaGun] = True;
$AutoUse[Mortar] = True;
$AutoUse[GrenadeLauncher] = True;
$AutoUse[LaserRifle] = True;
$AutoUse[EnergyRifle] = True;
$AutoUse[TargetingLaser] = False;
$AutoUse[ChargeGun] = True;

$Use[Blaster] = True;

$ArmorType[Male, LightArmor] = larmor;
$ArmorType[Male, MediumArmor] = marmor;
$ArmorType[Male, HeavyArmor] = harmor;
$ArmorType[Female, LightArmor] = lfemale;
$ArmorType[Female, MediumArmor] = mfemale;	   
$ArmorType[Female, HeavyArmor] = harmor;

$ArmorName[larmor] = LightArmor;
$ArmorName[marmor] = MediumArmor;
$ArmorName[harmor] = HeavyArmor;
$ArmorName[lfemale] = LightArmor;
$ArmorName[mfemale] = MediumArmor;

// Amount to remove when selling or dropping ammo
$SellAmmo[BulletAmmo] = 25;
$SellAmmo[PlasmaAmmo] = 5;
$SellAmmo[DiscAmmo] = 5;
$SellAmmo[GrenadeAmmo] = 5;
$SellAmmo[MortarAmmo] = 5;
$SellAmmo[Beacon] = 5;
$SellAmmo[MineAmmo] = 5;
$SellAmmo[Grenade] = 5;

// Max Amount of ammo the Ammo Pack can carry
$AmmoPackMax[BulletAmmo] = 150;
$AmmoPackMax[PlasmaAmmo] = 30;
$AmmoPackMax[DiscAmmo] = 15;
$AmmoPackMax[GrenadeAmmo] = 15;
$AmmoPackMax[MortarAmmo] = 10;
$AmmoPackMax[MineAmmo] = 5;
$AmmoPackMax[Grenade] = 10;
$AmmoPackMax[Beacon] = 10;

// Items in the AmmoPack
$AmmoPackItems[0] = BulletAmmo;
$AmmoPackItems[1] = PlasmaAmmo;
$AmmoPackItems[2] = DiscAmmo;
$AmmoPackItems[3] = GrenadeAmmo;
$AmmoPackItems[4] = Grenade;
$AmmoPackItems[5] = MineAmmo;
$AmmoPackItems[6] = MortarAmmo;
$AmmoPackItems[7] = Beacon;

// Limit on number of special Items you can buy
$TeamItemMax[DeployableAmmoPack] = 7;
$TeamItemMax[DeployableInvPack] = 5;
$TeamItemMax[TurretPack] = 10;
$TeamItemMax[CameraPack] = 15;
$TeamItemMax[DeployableSensorJammerPack] = 8;
$TeamItemMax[PulseSensorPack] = 15;
$TeamItemMax[MotionSensorPack] = 15;
$TeamItemMax[ScoutVehicle] = 3;
$TeamItemMax[HAPCVehicle] = 1;
$TeamItemMax[LAPCVehicle] = 2;
$TeamItemMax[Beacon] = 40;
$TeamItemMax[mineammo] = 35;

// Weapon to ammo table
$WeaponAmmo[Blaster] = "";
$WeaponAmmo[PlasmaGun] = PlasmaAmmo;
$WeaponAmmo[Chaingun] = BulletAmmo;
$WeaponAmmo[DiscLauncher] = DiscAmmo;
$WeaponAmmo[GrenadeLauncher] = GrenadeAmmo;
$WeaponAmmo[Mortar] = Mortar;
$WeaponAmmo[LaserRifle] = "";
$WeaponAmmo[EnergyRifle] = "";


//----------------------------------------------------------------------------
// Server side methods
// The client side inventory dialogs call buyItem, sellItem,
// useItem and dropItem through remoteEvals.

function teamEnergyBuySell(%player,%cost)
{
	%client = Player::getClient(%player);
	%team = Client::getTeam(%client);
	// IF - Cost positive selling    IF - Cost Negitive buying 
	%station = %player.Station;
	%stationName = GameBase::getDataName(%station); 
	if(%stationName == DeployableInvStation || %stationName == DeployableAmmoStation) {
		%station.Energy += %cost;			//Remote StationEnergy
		if(%station.Energy < 1)
			%station.Energy = 0;
	}
	else if($TeamEnergy[%team] != "Infinite") { 
		$TeamEnergy[%team] += %cost;    //Total TeamEnergy
 		%client.teamEnergy += %cost;   //Personal TeamEnergy
	}
}

function isPlayerBusy(%client)
{
	// Can't buy things if busy shooting.
	%state = Player::getItemState(%client,$WeaponSlot);
	return %state == "Fire" || %state == "Reload";
}

function remoteBuyFavorites(%client,%favItem0,%favItem1,%favItem2,%favItem3,%favItem4,%favItem5,%favItem6,%favItem7,%favItem8,%favItem9,%favItem10,%favItem11,%favItem12,%favItem13,%favItem14,%favItem15,%favItem16,%favItem17,%favItem18,%favItem19)
{
	if (isPlayerBusy(%client))
		return;

   // only can buy fav every 1/2 second
   %time = getIntegerTime(true) >> 4; // int half seconds
   if(%time <= %client.lastBuyFavTime)
      return;

   %client.lastBuyFavTime = %time;

	%station = (Client::getOwnedObject(%client)).Station;
	if(%station != "" ) {
		%stationName = GameBase::getDataName(%station); 
		if(%stationName == DeployableInvStation || %stationName == DeployableAmmoStation) 
			%energy = %station.Energy;
		else 
			%energy = $TeamEnergy[Client::getTeam(%client)];
		if(%energy == "Infinite" || %energy > 0) {
			%error = 0;
			%bought = 0;
			%max = getNumItems();
			for (%i = 0; %i < %max; %i = %i + 1) { 
				%item = getItemData(%i);
				if ($ServerCheats || Client::isItemShoppingOn(%client,%item)|| $TestCheats) {
					%count = Player::getItemCount(%client,%item);
					if(%count) {
						if(%item.className != Armor) 
							teamEnergyBuySell(Client::getOwnedObject(%client),(%item.price * %count));
						Player::setItemCount(%client, %item, 0);  
					}
				}
			}
			for (%i = 0; %i < 20; %i++) { 
				if(%favItem[%i] != "") {
					%item = getItemData(%favItem[%i]);
					if ((Client::isItemShoppingOn(%client,%item)) && ($ItemMax[Player::getArmor(%client),  %item] > Player::getItemCount(%client,%item) || %item.className == Armor)) {
						if(!buyItem(%client,%item))  
							%error = 1;
						else
							%bought++;
					}
				}
		  	}
			if(%bought) {
				if(%error) 
					Client::sendMessage(%client,0,"~wC_BuySell.wav");
				else 
					Client::SendMessage(%client,0,"~wbuysellsound.wav");
			}
			updateBuyingList(%client);
		}
	}
}


function replenishTeamEnergy(%team)
{
	$TeamEnergy[%team] += $incTeamEnergy;
	schedule("replenishTeamEnergy(" @ %team @ ");", $secTeamEnergy);
}


function checkResources(%player,%item,%delta,%noMessage)
{
	%client = Player::getClient(%player);
	%team = Client::getTeam(%client);
	%extraAmmo = 0 ;
	if (Player::getMountedItem(%client,$BackpackSlot) == ammopack && $AmmoPackMax[%item] != "") {
		%extraAmmo = $AmmoPackMax[%item];
		if(%delta == $ItemMax[Player::getArmor(%client), %item]) 
			%delta = %delta + %extraAmmo;
	}
	if($TestCheats == 0 && %client.spawn == "") {
		%energy = $TeamEnergy[%team];
    	%station = %player.Station;
		%sName = GameBase::getDataName(%station);
		if(%sName == DeployableInvStation || %sName == DeployableAmmoStation){
			%energy = %station.Energy;
		}
		if(%energy != "Infinite") {
			if (%item.price * %delta > %energy)	
				%delta = %energy / %item.price; 
			if(%delta < 1 ) {
				if(%noMessage == "")
					Client::sendMessage(%client,0,"Couldn't buy " @ %item.description @ " - "@ %energy @ " Energy points left");
				return 0;
			}
		}
	}
	if(%item.className == Weapon) {
		%armor = Player::getArmor(%client);
		%wcount = Player::getItemClassCount(%client,"Weapon");
		if (Player::getItemClassCount(%client,"Weapon") >= $MaxWeapons[%armor]) {
			Client::sendMessage(%client,0,"To many weapons for " @ $ArmorName[%armor].description @ " to carry");
			return 0;
		}
  	}
	else if(%item == RepairPatch) {
		%pDamage = GameBase::getDamageLevel(%player);
		if(GameBase::getDamageLevel(%player) > 0) 
			return 1;
		return 0;
   }
   else if($TeamItemMax[%item] != "" && !$TestCheats) {
		if($TeamItemMax[%item] <= $TeamItemCount[%team, %item]) {
			Client::sendMessage(%client,0,"Deployable Item limit reached for " @ %item.description @ "s");
			return 0;
		}
	}
	if(%item.className != Armor && %item.className != Vehicle) {
	   %count = Player::getItemCount(%client,%item);
	  	%max = $ItemMax[(Player::getArmor(%client)), %item] + %extraAmmo ;
	   if(%delta + %count >= %max) 
			%delta = %max - %count;
	}
	return %delta;
}

function buyItem(%client,%item)
{
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;
	
	%player = Client::getOwnedObject(%client);
	%armor = Player::getArmor(%client);
	
	if (($ServerCheats || Client::isItemShoppingOn(%client,%item) || $TestCheats || %client.spawn) && 
			($ItemMax[%armor, %item] || %item.className == Armor || %item.className == Vehicle || $TestCheats)) {
		if (%item.className == Armor) {
			// Assign armor by requested type & gender 
			%buyarmor = $ArmorType[Client::getGender(%client), %item];
			if(%armor != %buyarmor || Player::getItemCount(%client,%item) == 0)	{
				teamEnergyBuySell(%player,$ArmorName[%armor].price);
				if(checkResources(%player,%item,1)) {
					teamEnergyBuySell(%player,$ArmorName[%buyarmor].price * -1);
					Player::setArmor(%client,%buyarmor);
					checkMax(%client,%buyarmor);
					armorChange(%client);
     				Player::setItemCount(%client, $ArmorName[%armor], 0);  
     				Player::setItemCount(%client, %item, 1);  
					if (Player::getMountedItem(%client,$BackpackSlot) == ammopack) 
						fillAmmoPack(%client);	
					return 1;
				}

				teamEnergyBuySell(%player,$ArmorName[%armor].price * -1);
			}
		}
		else if (%item.className == Backpack) {
			if($TeamItemMax[%item] != "") {						
				if($TeamItemCount[GameBase::getTeam(%client) @ %item] >= $TeamItemMax[%item])
			 	  return 0;
			 }

			// Only one backpack per armor.
			%pack = Player::getMountedItem(%client,$BackpackSlot);
			if (%pack != -1) {
				if(%pack == ammopack) 
					checkMax(%client,%armor);
				else if(%pack == EnergyPack) {
					if(Player::getItemCount(%client,"LaserRifle") > 0) {
						Client::sendMessage(%client,0,"Sold Energy Pack - Auto Selling Laser Rifle");
						remoteSellItem(%client,22);						
					}
				}	
				teamEnergyBuySell(%player,%pack.price);
				Player::decItemCount(%client,%pack);
			}			   
			if (checkResources(%player,%item,1) || $testCheats) {
				teamEnergyBuySell(%player,%item.price * -1);
				Player::incItemCount(%client,%item);
				Player::useItem(%client,%item);									 
				if(%item == ammopack) 
					fillAmmoPack(%client);
				return 1;
			}
			else if(%pack != -1) {
				teamEnergyBuySell(%player,%pack.price * -1);
				Player::incItemCount(%client,%pack);
				Player::useItem(%client,%pack);									 
				if(%pack == ammopack) 
					fillAmmoPack(%client);
			}				 
		}
		else if(%item.className == Weapon) {
			if(checkResources(%player,%item,1)) {
				if(%item == LaserRifle && Player::getItemCount(%client,"EnergyPack") == 0) {
					buyItem(%client,"EnergyPack");
					Client::sendMessage(%client,0,"Bought Laser Rifle - Auto buying Energy Pack");
				}
				Player::incItemCount(%client,%item);
				teamEnergyBuySell(%player,(%item.price * -1));
				%ammoItem =  %item.imageType.ammoType; 
				if(%ammoItem != "") {
					%delta = checkResources(%player,%ammoItem,$ItemMax[%armor, %ammoItem]);
					if(%delta || $testCheats) {
						teamEnergyBuySell(%player,(%ammoItem.price * -1 * %delta));
						Player::incItemCount(%client,%ammoitem,%delta);
					}
				}
				return 1;
			}
		}
	 	else if(%item.className == Vehicle) {
		   if($TeamItemCount[GameBase::getTeam(%client) @ %item] < $TeamItemMax[%item]) {
				%shouldBuy = VehicleStation::checkBuying(%client,%item);
				if(%shouldBuy == 1) {
					teamEnergyBuySell(%player,(%item.price * -1));
					return 1;
				}			
 				else if(%shouldBuy == 2)
					return 1;
			}
		}
		else {
			if($TeamItemMax[%item] != "") {						
				if($TeamItemCount[GameBase::getTeam(%client) @ %item] >= $TeamItemMax[%item])
			 	  return 0;
			 }
		    %delta = checkResources(%player,%item,$ItemMax[%armor, %item]);
			 if(%delta || $testCheats) {
				teamEnergyBuySell(%player,(%item.price * -1 * %delta));
				Player::incItemCount(%client,%item,%delta);
				return 1;
			}
		}
		
 	}
	return 0;
}

function armorChange(%client)
{
	%player = Client::getOwnedObject(%client);
	if(%client.respawn == "" && %player.Station != "") {
		%sPos = GameBase::getPosition(%player.Station);
		%pPos	= GameBase::getPosition(%client);
		%posX = getWord(%sPos,0);
		%posY = getWord(%sPos,1);
		%posZ = getWord(%pPos,2);
		%vec = Vector::getFromRot(GameBase::getRotation(%player.Station),-1);	
	  	%newPosX = (getWord(%vec,0) * 1) + %posX;		 
		%newPosY = (getWord(%vec,1) * 1) + %posY;
		GameBase::setPosition(%client, %newPosX @ " " @ %newPosY @ " " @ %posZ);
	}
}

function remoteBuyItem(%client, %type)
{
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;

	%item = getItemData(%type);
	if(buyItem(%client,%item))
	{
		//afk monitor
		%client.lastActiveTimestamp = getSimTime();

 		
 		Client::sendMessage(%client,0,"~wbuysellsound.wav");
		updateBuyingList(%client);
	}
	else 
  		Client::sendMessage(%client,0,"You couldn't buy "@ %item.description @"~wC_BuySell.wav");
}

function remoteSellItem(%client,%type)
{
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;

	%item = getItemData(%type);
	%player = Client::getOwnedObject(%client);
	if ($ServerCheats || Client::isItemShoppingOn(%client,%item) || $TestCheats) {
		if(Player::getItemCount(%client,%item) && %item.className != Armor) {
			%numsell = 1;
			if(%item.className == Ammo || %item.className == HandAmmo) {
				%count = Player::getItemCount(%client, %item);
				if(%count < $SellAmmo[%item]) 
					%numsell = %count; 
				else 
					%numsell = $SellAmmo[%item];
			}
			else if (%item == ammopack) 
				checkMax(%client,Player::getArmor(%client));
			else if($TeamItemMax[%item] != "") {
				if(%item.className == Vehicle) 
					$TeamItemCount[(Client::getTeam(%client)) @ %item]--;
			}
			else if(%item == EnergyPack) { 
				if(Player::getItemCount(%client,"LaserRifle") > 0) {
					Client::sendMessage(%client,0,"Sold Energy Pack - Auto Selling Laser Rifle");
					remoteSellItem(%client,22);						
				}
			}
			teamEnergyBuySell(%player,%item.price * %numsell);
			Player::setItemCount(%player,%item,(%count-%numsell));
			updateBuyingList(%client);
			Client::SendMessage(%client,0,"~wbuysellsound.wav");
			
			//afk monitor
			%client.lastActiveTimestamp = getSimTime();
			
			return 1;
		}
	}
	Client::sendMessage(%client,0,"Cannot sell item ~wC_BuySell.wav");
}

//----------------------------------------------------------------------------

function remoteUseItem(%client,%type)
{
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;
	
	//echo("Use item: " @ %type @ " " @ %item);
	%client.throwStrength = 1;
	
	//afk monitor
	%client.lastActiveTimestamp = getSimTime();

	%item = getItemData(%type);
	if (%item == Backpack) 
		%item = Player::getMountedItem(%client,$BackpackSlot);
	else {
		if (%item == Weapon) 
			%item = Player::getMountedItem(%client,$WeaponSlot);
	}
	Player::useItem(%client,%item);
}

function remoteThrowItem(%client,%type,%strength)
{
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;

	%player = Client::getOwnedObject(%client);
	if(%player.Station == "" && %player.waitThrowTime + $WaitThrowTime <= getSimTime()) {
		if(GameBase::getControlClient(%player) != -1 || %player.vehicle != "") {
		//if(GameBase::getControlClient(%player) != -1) {
	  		//echo("Throw item: " @ %type @ " " @ %strength);
			%item = getItemData(%type);
			if (%item == Grenade || %item == MineAmmo) {
				if (%strength < 0)
					%strength = 0;
				else
					if (%strength > 100)
						%strength = 100;
				%client.throwStrength = 0.3 + 0.7 * (%strength / 100);
				Player::useItem(%client,%item);
			}
		}
	}
}

//anti spam shit
	
$MaxItemsperClient = 16;  // max a client can drop before litter flood is triggered    

function remoteDropItem(%client,%type)
{
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;

	if( %client.inInvStation )
	{      
		if( %client.dropItems < $MaxItemsperClient )
			%client.dropItems++;
		else
		{
			if (%client.lastDropTime == "")
			{
				%client.lastDropTime = getIntegerTime(true) >> 5; //getSimTime();
				return;
			}
			else
			{
				if ( ((getIntegerTime(true) >> 5) - %client.lastDropTime) > 5 )
				{
					%client.dropItems = 0;
					%client.lastDropTime = "";
				}
				else
					return;
			}
		}
	}

	if((Client::getOwnedObject(%client)).driver != 1)
	{
		//echo("Drop item: ",%type);
		%client.throwStrength = 1;

		%item = getItemData(%type);
		if (%item == Backpack)
		{
			%item = Player::getMountedItem(%client,$BackpackSlot);
			Player::dropItem(%client,%item);
		}
		else if (%item == Weapon)
		{
			%item = Player::getMountedItem(%client,$WeaponSlot);
			Player::dropItem(%client,%item);
		}
		else if (%item == Ammo)
		{
			%item = Player::getMountedItem(%client,$WeaponSlot);

			if(%item.className == Weapon)
			{
				%item = %item.imageType.ammoType;
				Player::dropItem(%client,%item);
			}
		}
		else 
		{
			Player::dropItem(%client,%item);
		}
	}
}
function remoteDeployItem(%client,%type)
{
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;

    //echo("Deploy item: ",%type);
	%item = getItemData(%type);
	Player::deployItem(%client,%item);
}

//----------------------------------------------------------------------------

$NextWeapon[EnergyRifle] = Blaster;
$NextWeapon[Blaster] = PlasmaGun;
$NextWeapon[PlasmaGun] = Chaingun;
$NextWeapon[Chaingun] = DiscLauncher;
$NextWeapon[DiscLauncher] = GrenadeLauncher;
$NextWeapon[GrenadeLauncher] = Mortar;
$NextWeapon[Mortar] = LaserRifle;
$NextWeapon[LaserRifle] = EnergyRifle;

$PrevWeapon[Blaster] = EnergyRifle;
$PrevWeapon[PlasmaGun] = Blaster;
$PrevWeapon[Chaingun] = PlasmaGun;
$PrevWeapon[DiscLauncher] = Chaingun;
$PrevWeapon[GrenadeLauncher] = DiscLauncher;
$PrevWeapon[Mortar] = GrenadeLauncher;
$PrevWeapon[LaserRifle] = Mortar;
$PrevWeapon[EnergyRifle] = LaserRifle;

function remoteNextWeapon(%client)
{
if(!(%client.observerMode == "" || %client.observerMode == "pregame"))
{
		return;
}
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;

	%item = Player::getMountedItem(%client,$WeaponSlot);
	if (%item == -1 || $NextWeapon[%item] == "")
		selectValidWeapon(%client);
	else {
		for (%weapon = $NextWeapon[%item]; %weapon != %item;
				%weapon = $NextWeapon[%weapon]) {
			if (isSelectableWeapon(%client,%weapon)) {
				Player::useItem(%client,%weapon);
				// Make sure it mounted (laser may not), or at least
				// next in line to be mounted.
				if (Player::getMountedItem(%client,$WeaponSlot) == %weapon ||
						Player::getNextMountedItem(%client,$WeaponSlot) == %weapon)
					break;
			}
		}
	}
}

function remotePrevWeapon(%client)
{
if(!(%client.observerMode == "" || %client.observerMode == "pregame"))
{
		return;
}
	%client = Player::getClient(%client);
	if( !%client || %client == -1)
		return;

	%item = Player::getMountedItem(%client,$WeaponSlot);
	if (%item == -1 || $PrevWeapon[%item] == "")
		selectValidWeapon(%client);
	else {
		for (%weapon = $PrevWeapon[%item]; %weapon != %item;
				%weapon = $PrevWeapon[%weapon]) {
			if (isSelectableWeapon(%client,%weapon)) {
				Player::useItem(%client,%weapon);
				// Make sure it mounted (laser may not), or at least
				// next in line to be mounted.
				if (Player::getMountedItem(%client,$WeaponSlot) == %weapon ||
						Player::getNextMountedItem(%client,$WeaponSlot) == %weapon)
					break;
			}
		}
	}
}

function selectValidWeapon(%client)
{
	%item = EnergyRifle;
	for (%weapon = $NextWeapon[%item]; %weapon != %item;
			%weapon = $NextWeapon[%weapon]) {
		if (isSelectableWeapon(%client,%weapon)) {
			Player::useItem(%client,%weapon);
			break;
		}
	}
}

function isSelectableWeapon(%client,%weapon)
{
	if (Player::getItemCount(%client,%weapon)) {
		%ammo = $WeaponAmmo[%weapon];
		if (%ammo == "" || Player::getItemCount(%client,%ammo) > 0)
			return true;
	}
	return false;
}


//----------------------------------------------------------------------------
// Default item scripts
//----------------------------------------------------------------------------

function Item::giveItem(%player,%item,%delta)
{
	%armor = Player::getArmor(%player);

	if($ItemMax[%armor, %item])
	{
		%client = Player::getClient(%player);
		if (%item.className == Backpack)
		{
			// Only one backpack per armor, and it's always mounted
			if (Player::getMountedItem(%player,$BackpackSlot) == -1)
			{
				Player::incItemCount(%player,%item);
				Player::useItem(%player,%item);
				Client::sendMessage(%client,0,"You received a " @ %item @ " backpack");
				
				//active
				zadmin::ActiveMessage::Single(%client, ItemReceived, %item, 1);
				return 1;
			}
		}
		else
		{
			// Check num weapons carried by player can't have more then max
			if (%item.className == Weapon)
			{
				if (Player::getItemClassCount(%player,"Weapon") >= $MaxWeapons[%armor]) 
				return 0;
			}  
			
			%extraAmmo = 0 ;
			
			if (Player::getMountedItem(%client,$BackpackSlot) == ammopack && $AmmoPackMax[%item] != "")
				%extraAmmo = $AmmoPackMax[%item];
			
			// Make sure it doesn't exceed carrying capacity
			%count = Player::getItemCount(%player,%item);
			if (%count + %delta > $ItemMax[%armor, %item] + %extraAmmo) 
				%delta = ($ItemMax[%armor, %item] + %extraAmmo) - %count;
			
			if (%delta > 0)
			{
				Player::incItemCount(%player,%item,%delta);
	
				if (%count == 0 && $AutoUse[%item]) 
				Player::useItem(%player,%item);
				Client::sendMessage(%client,0,"You received " @ %delta @ " " @ %item.description);
				
				//active
				zadmin::ActiveMessage::Single(%client, ItemReceived, %item.description, %delta);
				
				return %delta;
			}
		}
	}
	
	return 0;
}

//----------------------------------------------------------------------------
// Default Item object methods

$PickupSound[Ammo] = "SoundPickupAmmo";
$PickupSound[Weapon] = "SoundPickupWeapon";
$PickupSound[Backpack] = "SoundPickupBackpack";
$PickupSound[Repair] = "SoundPickupHealth";

function Item::playPickupSound(%this)
{
	%item = Item::getItemData(%this);
	%sound = $PickupSound[%item.className];
	if (%sound != "")  
		playSound(%sound,GameBase::getPosition(%this));
	else {
		// Generic item sound
		playSound(SoundPickupItem,GameBase::getPosition(%this));
	}
}	

function Item::respawn(%this)
{
	// If the item is rotating we respawn it,
	if (Item::isRotating(%this)) {
		Item::hide(%this,True);
		schedule("Item::hide(" @ %this @ ",false); GameBase::startFadeIn(" @ %this @ ");",$ItemRespawnTime,%this);
	}
	else { 
		deleteObject(%this);
	}
}	

function Item::onAdd(%this)
{
}

function Item::onCollision(%this,%object)
{
	if (getObjectType(%object) == "Player") {
		%item = Item::getItemData(%this);
		%count = Player::getItemCount(%object,%item);
		if (Item::giveItem(%object,%item,Item::getCount(%this))) {
			Item::playPickupSound(%this);
			Item::respawn(%this);
		}
	}
}


function Item::onMount(%player,%item)
{
}

function Item::onUnmount(%player,%item)
{
}

function Item::onUse(%player,%item)
{
	//echo("Item used: ",%player," ",%item);
	Player::mountItem(%player,%item,$DefaultSlot);
}

function Item::pop(%item)
{
 	GameBase::startFadeOut(%item);
   schedule("deleteObject(" @ %item @ ");",2.5, %item);
}

function Item::onDrop(%player,%item)
{
	if($matchStarted) {
		if(%item.className != Armor) {
			//echo("Item dropped: ",%player," ",%item);
			%obj = newObject("","Item",%item,1,false);
 	 	  	schedule("Item::Pop(" @ %obj @ ");", $ItemPopTime, %obj);
 	 	 	addToSet("MissionCleanup", %obj);
			if (Player::isDead(%player)) 
				GameBase::throw(%obj,%player,10,true);
			else {
				GameBase::throw(%obj,%player,15,false);
				Item::playPickupSound(%obj);
			}
			Player::decItemCount(%player,%item,1);
			return %obj;
		}
	}
}

function Item::onDeploy(%player,%item,%pos)
{
}


//----------------------------------------------------------------------------

function Flag::onUse(%player,%item)
{
	Player::mountItem(%player,%item,$FlagSlot);
}

//----------------------------------------------------------------------------

function Weapon::onDrop(%player,%item)
{
	%state = Player::getItemState(%player,$WeaponSlot);
	if (%state != "Fire" && %state != "Reload")
		Item::onDrop(%player,%item);
}	

function Weapon::onUse(%player,%item)
{
	if(%player.Station==""){
		%ammo = %item.imageType.ammoType;
		if (%ammo == "") {
			// Energy weapons dont have ammo types
			Player::mountItem(%player,%item,$WeaponSlot);
		}
		else {
			if (Player::getItemCount(%player,%ammo) > 0) 
				Player::mountItem(%player,%item,$WeaponSlot);
			else {
				Client::sendMessage(Player::getClient(%player),0,
				strcat(%item.description," has no ammo"));
			}
		}
	}
}


//----------------------------------------------------------------------------

function Ammo::onDrop(%player,%item)
{
	if($matchStarted) {
		%count = Player::getItemCount(%player,%item);
		%delta = $SellAmmo[%item];
		if(%count <= %delta) { 
			if( %item == BulletAmmo || (Player::getMountedItem(%player,$WeaponSlot)).imageType.ammoType != %item)
				%delta = %count;
			else 
				%delta = %count - 1;

		}
		if(%delta > 0) {
			%obj = newObject("","Item",%item,%delta,false);
      	schedule("Item::Pop(" @ %obj @ ");", $ItemPopTime, %obj);

      	addToSet("MissionCleanup", %obj);
			GameBase::throw(%obj,%player,20,false);
			Item::playPickupSound(%obj);
			Player::decItemCount(%player,%item,%delta);
		}
	}
}	

//----------------------------------------------------------------------------

function Tool::onUse(%player,%item)
{
	Player::mountItem(%player,%item,$ToolSlot);
}

//----------------------------------------------------------------------------

function checkMax(%client,%armor)
{
 	%weaponflag = 0;
	%numweapon = Player::getItemClassCount(%client,"Weapon");
	if (%numweapon > $MaxWeapons[%armor]) {
	   %weaponflag = %numweapon - $MaxWeapons[%armor];
	}
	%max = getNumItems();
	for (%i = 0; %i < %max; %i = %i + 1) {
		%item = getItemData(%i);
		%maxnum = $ItemMax[%armor, %item];
		if(%maxnum != "") {
			%numsell = 0;
			%count = Player::getItemCount(%client,%item);
			if(%count > %maxnum) {
				%numsell =  %count - %maxnum;
			}
			if (%count > 0 && %weaponflag && %item.className == Weapon) {
				%numsell = 1;
				%weaponflag = %weaponflag - 1;
			}
			if(%numsell > 0) {
		    	Client::sendMessage(%client,0,"SOLD " @ %numsell @ " " @ %item);
				teamEnergyBuySell(Client::getOwnedObject(%client),(%item.price * %numsell));
				Player::setItemCount(%client, %item, %count - %numsell);  
				updateBuyingList(%client);
			} 
		}
	}
}

function checkPlayerCash(%client)
{
	%team = Client::getTeam(%client);	
	if($TeamEnergy[%team] != "Infinite") {
		if(%client.teamEnergy > ($InitialPlayerEnergy * -1) ) {
			if(%client.teamEnergy >= 0)
				%diff = $InitialPlayerEnergy;
			else 
				%diff = $InitialPlayerEnergy + %client.teamEnergy;
			$TeamEnergy[%team] -= %diff;
		}
	}
}	

function Mission::reinitData()
{
	$TeamItemCount[0 @ DeployableAmmoPack] = 0;
	$TeamItemCount[0 @ DeployableInvPack] = 0;
	$TeamItemCount[0 @ TurretPack] = 0;
	$TeamItemCount[0 @ CameraPack] = 0;
	$TeamItemCount[0 @ DeployableSensorJammerPack] = 0;
	$TeamItemCount[0 @ PulseSensorPack] = 0;
	$TeamItemCount[0 @ MotionSensorPack] = 0;
	$TeamItemCount[0 @ ScoutVehicle] = 0;
	$TeamItemCount[0 @ LAPCVehicle] = 0;
	$TeamItemCount[0 @ HAPCVehicle] = 0;
	$TeamItemCount[0 @ Beacon] = 0;
	$TeamItemCount[0 @ mineammo] = 0;

	$TeamItemCount[1 @ DeployableAmmoPack] = 0;
	$TeamItemCount[1 @ DeployableInvPack] = 0;
	$TeamItemCount[1 @ TurretPack] = 0;
	$TeamItemCount[1 @ CameraPack] = 0;
	$TeamItemCount[1 @ DeployableSensorJammerPack] = 0;
	$TeamItemCount[1 @ PulseSensorPack] = 0;
	$TeamItemCount[1 @ MotionSensorPack] = 0;
	$TeamItemCount[1 @ ScoutVehicle] = 0;
	$TeamItemCount[1 @ LAPCVehicle] = 0;
	$TeamItemCount[1 @ HAPCVehicle] = 0;
	$TeamItemCount[1 @ Beacon] = 0;
	$TeamItemCount[1 @ mineammo] = 0;

	$TeamItemCount[2 @ DeployableAmmoPack] = 0;
	$TeamItemCount[2 @ DeployableInvPack] = 0;
	$TeamItemCount[2 @ TurretPack] = 0;
	$TeamItemCount[2 @ CameraPack] = 0;
	$TeamItemCount[2 @ DeployableSensorJammerPack] = 0;
	$TeamItemCount[2 @ PulseSensorPack] = 0;
	$TeamItemCount[2 @ MotionSensorPack] = 0;
	$TeamItemCount[2 @ ScoutVehicle] = 0;
	$TeamItemCount[2 @ LAPCVehicle] = 0;
	$TeamItemCount[2 @ HAPCVehicle] = 0;
	$TeamItemCount[2 @ Beacon] = 0;
	$TeamItemCount[2 @ mineammo] = 0;

	$TeamItemCount[3 @ DeployableAmmoPack] = 0;
	$TeamItemCount[3 @ DeployableInvPack] = 0;
	$TeamItemCount[3 @ TurretPack] = 0;
	$TeamItemCount[3 @ CameraPack] = 0;
	$TeamItemCount[3 @ DeployableSensorJammerPack]= 0;
	$TeamItemCount[3 @ PulseSensorPack] = 0;
	$TeamItemCount[3 @ MotionSensorPack] = 0;
	$TeamItemCount[3 @ ScoutVehicle] = 0;
	$TeamItemCount[3 @ LAPCVehicle] = 0;
	$TeamItemCount[3 @ HAPCVehicle] = 0;
	$TeamItemCount[3 @ Beacon] = 0;
	$TeamItemCount[3 @ mineammo] = 0;

	$TeamItemCount[4 @ DeployableAmmoPack] = 0;
	$TeamItemCount[4 @ DeployableInvPack] = 0;
	$TeamItemCount[4 @ TurretPack] = 0;
	$TeamItemCount[4 @ CameraPack] = 0;
	$TeamItemCount[4 @ DeployableSensorJammerPack]= 0;
	$TeamItemCount[4 @ PulseSensorPack] = 0;
	$TeamItemCount[4 @ MotionSensorPack] = 0;
	$TeamItemCount[4 @ ScoutVehicle] = 0;
	$TeamItemCount[4 @ LAPCVehicle] = 0;
	$TeamItemCount[4 @ HAPCVehicle] = 0;
	$TeamItemCount[4 @ Beacon] = 0;
	$TeamItemCount[4 @ mineammo] = 0;

	$TeamItemCount[5 @ DeployableAmmoPack] = 0;
	$TeamItemCount[5 @ DeployableInvPack] = 0;
	$TeamItemCount[5 @ TurretPack] = 0;
	$TeamItemCount[5 @ CameraPack] = 0;
	$TeamItemCount[5 @ DeployableSensorJammerPack]= 0;
	$TeamItemCount[5 @ PulseSensorPack] = 0;
	$TeamItemCount[5 @ MotionSensorPack] = 0;
	$TeamItemCount[5 @ ScoutVehicle] = 0;
	$TeamItemCount[5 @ LAPCVehicle] = 0;
	$TeamItemCount[5 @ HAPCVehicle] = 0;
	$TeamItemCount[5 @ Beacon] = 0;
	$TeamItemCount[5 @ mineammo] = 0;

	$TeamItemCount[6 @ DeployableAmmoPack] = 0;
	$TeamItemCount[6 @ DeployableInvPack] = 0;
	$TeamItemCount[6 @ TurretPack] = 0;
	$TeamItemCount[6 @ CameraPack] = 0;
	$TeamItemCount[6 @ DeployableSensorJammerPack]= 0;
	$TeamItemCount[6 @ PulseSensorPack] = 0;
	$TeamItemCount[6 @ MotionSensorPack] = 0;
	$TeamItemCount[6 @ ScoutVehicle] = 0;
	$TeamItemCount[6 @ LAPCVehicle] = 0;
	$TeamItemCount[6 @ HAPCVehicle] = 0;
	$TeamItemCount[6 @ Beacon] = 0;
	$TeamItemCount[6 @ mineammo] = 0;

	$TeamItemCount[7 @ DeployableAmmoPack] = 0;
	$TeamItemCount[7 @ DeployableInvPack] = 0;
	$TeamItemCount[7 @ TurretPack] = 0;
	$TeamItemCount[7 @ CameraPack] = 0;
	$TeamItemCount[7 @ DeployableSensorJammerPack]= 0;
	$TeamItemCount[7 @ PulseSensorPack] = 0;
	$TeamItemCount[7 @ MotionSensorPack] = 0;
	$TeamItemCount[7 @ ScoutVehicle] = 0;
	$TeamItemCount[7 @ LAPCVehicle] = 0;
	$TeamItemCount[7 @ HAPCVehicle] = 0;
	$TeamItemCount[7 @ Beacon] = 0;
	$TeamItemCount[7 @ mineammo] = 0;

	$totalNumCameras = 0;
	$totalNumTurrets = 0;

	for(%i = -1; %i < 8 ; %i++)
		$TeamEnergy[%i] = $DefaultTeamEnergy; 
}


function LaserRifle::onUse(%player,%item)
{
	if(Player::getMountedItem(%player,$BackpackSlot) == EnergyPack)
		Weapon::onUse(%player,%item);
	else
		Client::sendMessage(Player::getClient(%player),0,
			"Must have an Energy Pack to use Laser Rifle."); 
}

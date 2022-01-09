// Freeze.cs
// v 3.02 - 2004/02/03
// Module for Tribes servers used to pause/resume game. Written as part of FSTAT v2.x, here modified for standalone purposes.
// By Slitz
// Modified By: KiLLeR2001 for PU Servers
// v 4.0 - 12/19/2021
// --------------------------------------------------------------------------------------------------------------------------
// ** Install notes **
// 1. Make sure this file is executed during server start.
// 2. Add the menu options (exactly how depends on your server. the following is an example)
//		I. Add these lines to function Game::menuRequest under the options menu:
//			if(!$freezedata::actice && $matchStarted && $Server::timelimit > 0)Client::addMenuItem(%clientId, %curItem++ @ "Pause game", "pause");
//			else if($freezedata::actice)Client::addMenuItem(%clientId, %curItem++ @ "Resume game", "pauseresume");
//		II. Add these lines to function processMenuOptions:
//			if(%opt == "pause")freeze::start(%clientId); - %clientId is the id of the admin
//			if(%opt == "pauseresume")freeze::stop(%clientId); - same as above
// 3. Make other necessary adjustments
//		I. Add this line to function processMenuPickTeam:
//			if($freezedata::actice && !Observer::isObserver(%clientId))return;
//		II. Add this line to function Game::playerSpawn to force joining players into pause
//			if($freezedata::actice)freeze::init(%clientId);
//		III. Add this line to the top of Game::checkTimeLimit
//			if($freezedata::actice){schedule("Game::checkTimeLimit();", 10);return;}
//		IV. Make sure paused players can't take damage or die by adding this line at the top of functions Player::onDamage and remoteKill:
//			if($freezedata::actice)return;
//		V. IMPORTANT STEPS: Make sure stuff is cleaned up when players drop during pause. In Server::onClientDisconnect add these lines:
//			if(%clientId.frozen) {
//				%player = $freezedata::realobj[%clientId];
//				removeFromSet(MissionCleanup,%player);
//				deleteObject(%player);
//				removeFromSet(MissionCleanup,$freezedata::camId[%clientId]);
//				deleteObject($freezedata::camId[%clientId]);
//				$freezedata::camId[%clientId] = "";
//			}
//			Also add this line to Server::onClientConnect to reset the player's paused state:
//				%clientid.frozen = false;
//		VI. Add this line anywhere in objectives.cs (just to make sure pause mode is reset on mission change)
//				if($freezedata::actice)freeze::stopNow();
//				
// 4. Things to consider:
// 		It's a good idea to force observermode "observerFly" or disable observing all together while $freezedata::actice to keep people from spying on enemy positions
//		Do this by eg adding if($freezedata::actice)return; in functions Observer::jump and Observer::enterObserverMode
//		You can do a "if($freezedata::actice)return;" in functions like remotePrevWeapon and remoteNextWeapon to make sure things are really frozen while in pause
//
// If I forgot something, PM me(slitz) in IRC.
// HF etc.

echo ("Running freeze.cs V4.0");

function freeze::reset()
{
    $freezedata::prevent = 0;
    $freezedata::actice = 0;
    $freezedata::cnt = 0;
    $freezedata::totaltime = 0;
    $freezedata::delay = 3; // set to delay in secs from admin starts it to actual activation
}

freeze::reset();

function freeze::start(%cl)
{
    
    if($FlagIsDropped[0] || $FlagIsDropped[1])
        return;
    
    if($freezedata::actice || $NoFlagThrow)
        return;
    
    if ($freeze::OOB[0] || $freeze::OOB[1])
        return;
    
    //IF PAUSE INITIATED, USERS CANT THROW FLAG LAST SECOND
    $NoFlagThrow = true;
    
    $freezedata::admin = Client::getName(%cl);
    
    if($freezedata::delay && $freezedata::prevent == 0) {
        $freezedata::prevent = 1;
        //$freezedata::starttime = getIntegerTime(true) >> 5;
        $freezedata::starttime = getSimTime();
        //messageAll(0, "The game will be paused in " @ $freezedata::delay @ " second(s)...~wmine_act.wav");
        //Schedule("freeze::all();", $freezedata::delay);
        freeze::all();
    }
}

function freeze::all()
{

        //freeze::disabledamage();
        $freezedata::actice = 1;
        
        for (%cl = Client::getfirst(); %cl != -1; %cl = Client::getnext(%cl))
        {
            freeze::init(%cl);
        }
        
        $freezedata::cnt++;
        messageAll(0, "Game Paused.~wCapturedTower.wav");
        UpdateClientTimes(0);
}

function freeze::init(%id)
{
        %weapon = Player::getMountedItem(%id,$WeaponSlot);
        $freezedata::realamo[%id] = $WeaponAmmo[%weapon];
        $freezedata::realact[%id] = Player::getItemCount(%id, $WeaponAmmo[%weapon]);
        $freezedata::realtms[%id] = Client::getTeam(%id);
        $freezedata::realpos[%id] = gamebase::getposition(%id);
        $freezedata::realrot[%id] = gamebase::getrotation(%id);
        $freezedata::realnrg[%id] = GameBase::getEnergy(client::getownedobject(%id));
        $freezedata::realvcy[%id] = item::getvelocity(%id);
        $freezedata::realcbj[%id] = client::getcontrolobject(%id);
        $freezedata::realobj[%id] = client::getOwnedObject(%id);
        $freezedata::realfsh[%id] = Player::getDamageFlash(%id);
        
        schedule("freeze::keep(" @ %id @");",0.1);
//        echo($freezedata::realpos[%id]);
}

function freeze::setpos(%id)
{
        gamebase::setposition(%id,$freezedata::newpos[%id]);
}

function freeze::playerStand(%playerid)
{
    %camobj = newobject("freezeCamStand"@%playerid,"Turret","CameraTurret",false);
    addtoset("MissionCleanup", %camobj);
    $freezedata::camId[%playerid] = %camobj;

    %pos = GameBase::GetPosition(%playerid);
    %posx = GetWord(%pos, 0);
    %posy = GetWord(%pos, 1);
    %posz = GetWord(%pos, 2)-2;
    %newpos = %posx @ " " @ %posy @ " "@ %posz;

    GameBase::SetPosition(%camobj, %newpos);
    
}

function freeze::keep(%id)

{
        if ($freezedata::actice == 1)
        {
            remoteeval(%id, CP, "<jc><f0>The game has been paused by " @ $freezedata::admin @ "\n\n<f2>Waiting to be resumed by an admin...");
            
            %id.frozen = true;
            //Client::setOwnedObject(%id, -1);
            Player::trigger(%id, $WeaponSlot, false);
            item::setvelocity(%id, 0);
            freeze::playerStand(%id);
            Client::setControlObject(%id, Client::getObserverCamera(%id));
            
            Observer::setOrbitObject(%id, %id, -1, -1, -1);
            remotePlayMode(%id);
            Client::setOwnedObject(%id, -1);
            
        }
        else
        {
        Client::sendMessage(%id, 0, "Game Resumed. (matchclock adjusted)~wmine_act.wav");
        if(!%id.frozen)
            return;

        removeFromSet(MissionCleanup,$freezedata::camId[%id]);
        deleteObject($freezedata::camId[%id]);
        $freezedata::camId[%id] = "";
       
        %id.frozen = false;
        
        client::setcontrolobject(%id, $freezedata::realcbj[%id]);
        Client::setOwnedObject(%id, $freezedata::realobj[%id]);
        Player::setDamageFlash(%id, $freezedata::realfsh[%id]);
        gamebase::setposition(%id, $freezedata::realpos[%id]);
        gamebase::setrotation(%id, $freezedata::realrot[%id]);
        item::setvelocity(%id, $freezedata::realvcy[%id]);
        gamebase::setEnergy(client::getownedobject(%id), $freezedata::realnrg[%id]);
        
        if(Client::getGuiMode(%id) != 1)
            Client::setGuiMode(%id, 1);
        Player::setItemCount(%id, $freezedata::realamo[%id], $freezedata::realact[%id]);
        centerprint(%id, "");       
        
        
        }

}

function freeze::stopcommand()
{
    $freezedata::actice = 0;
    for (%cl = Client::getfirst(); %cl != -1; %cl = Client::getnext(%cl))
    {
        freeze::keep(%cl);
    }
    //%stoptime = getIntegerTime(true) >> 5;
    %stoptime = getSimTime();
    %totaltime = (%stoptime - $freezedata::starttime);
    //%totaltime = ($freezedata::starttime - $missionStartTime);
    $missionStartTime += %totaltime;
    $freezedata::cnt++;
    
    //%curTimeLeft = ($Server::timeLimit * 60) + $missionStartTime - %stoptime;
    
    //
    //%curTimeLeft = ($Server::timeLimit * 60) + (%totaltime - %stoptime);
    //

    //messageAll(0, "FREEZETimeAfter: " @ %curTimeLeft);
    //UpdateClientTimes(%curTimeLeft);
    
    Game::checkTimeLimit();
    
    deletevariables("$freezedata::real*");
    
    //RESUME ABILITY TO TOSS FLAG
    $NoFlagThrow = false;
    $freezedata::prevent = 0;
}

function freeze::stop(%cl)
{
    if(!$freezedata::actice)
        return;
    
    schedule("freeze::stopcommand();",5);
    schedule("messageAll(0, \"Prepare to play in 1 second...\");",4);
    schedule("messageAll(0, \"Prepare to play in 2 seconds...\");",3);
    schedule("messageAll(0, \"Prepare to play in 3 seconds...\");",2);
    schedule("messageAll(0, \"Prepare to play in 4 seconds...\");",1);
    messageAll(0, "Prepare to play in 5 seconds...~wsensor_deploy.wav");
    centerprintall("<jc><f1>Game was resumed by " @ $freezedata::admin,5);
    // messageAll(0, "Prepare to play in 10 seconds...~wmine_act.wav");
}

function freeze::posstop(%id)
{
    gamebase::setposition(%id,$freezedata::realpos[%id]);
    gamebase::setrotation(%id,$freezedata::realrot[%id]);
    gamebase::setEnergy(client::getownedobject(%id), $freezedata::realnrg[%id]);
}

function freeze::stopNow()
{
    freeze::reset();
    for (%cl = Client::getfirst(); %cl != -1; %cl = Client::getnext(%cl))
    {
        %cl.observerMode = "";
    }
}
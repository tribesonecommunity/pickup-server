exec("code.chat.cs");

//suicide multiplier, greater the number less frequent time checks
$SuicideMult = 0.5;

function remotePlayMode(%clientId)
{
  if(!%clientId.guiLock)
  {
    remoteSCOM(%clientId, -1);
    Client::setGuiMode(%clientId, $GuiModePlay);
  }
}

function remoteCommandMode(%clientId)
{
  // can't switch to command mode while a server menu is up
  if(!%clientId.guiLock)
  {
    remoteSCOM(%clientId, -1);  // force the bandwidth to be full command
     if(%clientId.observerMode != "pregame")
        checkControlUnmount(%clientId);
      Client::setGuiMode(%clientId, $GuiModeCommand);
  }
}

function remoteInventoryMode(%clientId)
{
   if(!%clientId.guiLock && !Observer::isObserver(%clientId))
   {
      remoteSCOM(%clientId, -1);
      Client::setGuiMode(%clientId, $GuiModeInventory);
   }
}

function remoteObjectivesMode(%clientId)
{
  if(!%clientId.guiLock)
  {
    remoteSCOM(%clientId, -1);
    Client::setGuiMode(%clientId, $GuiModeObjectives);
  }
}

function remoteScoresOn(%clientId)
{
  if(!%clientId.menuMode)
    Game::menuRequest(%clientId);
}

function remoteScoresOff(%clientId)
{
  Client::cancelMenu(%clientId);
}

function remoteToggleCommandMode(%clientId)
{
    if (Client::getGuiMode(%clientId) != $GuiModeCommand)
        remoteCommandMode(%clientId);
    else
        remotePlayMode(%clientId);
}

function remoteToggleInventoryMode(%clientId)
{
    if (Client::getGuiMode(%clientId) != $GuiModeInventory)
        remoteInventoryMode(%clientId);
    else
        remotePlayMode(%clientId);
}

function remoteToggleObjectivesMode(%clientId)
{
    if (Client::getGuiMode(%clientId) != $GuiModeObjectives)
        remoteObjectivesMode(%clientId);
    else
        remotePlayMode(%clientId);
}

function Time::getMinutes(%simTime)
{
   return floor(%simTime / 60);
}

function Time::getSeconds(%simTime)
{
   return %simTime % 60;
}

function Game::pickRandomSpawn(%team)
{
    
    
   %group = nameToID("MissionGroup/Teams/team" @ %team @ "/DropPoints/Random");
   %count = Group::objectCount(%group);
   if(!%count)
      return -1;
    %spawnIdx = floor(getRandom() * (%count - 0.1));
    %value = %count;
    for(%i = %spawnIdx; %i < %value; %i++) {
        %set = newObject("set",SimSet);
        %obj = Group::getObject(%group, %i);
        if(containerBoxFillSet(%set,$SimPlayerObjectType|$VehicleObjectType,GameBase::getPosition(%obj),2,2,4,0) == 0) {
            deleteObject(%set);
            return %obj;
        }
        if(%i == %count - 1) {
            %i = -1;
            %value = %spawnIdx;
        }
        deleteObject(%set);
    }
   return false;
}

function Game::pickStartSpawn(%team)
{
   %group = nameToID("MissionGroup\\Teams\\team" @ %team @ "\\DropPoints\\Start");
   %count = Group::objectCount(%group);
   if(!%count)
      return -1;

   %spawnIdx = $lastTeamSpawn[%team] + 1;
   if(%spawnIdx >= %count)
      %spawnIdx = 0;
   $lastTeamSpawn[%team] = %spawnIdx;
   return Group::getObject(%group, %spawnIdx);
}

function Game::pickTeamSpawn(%team, %respawn)
{
    
   if(%respawn)
      return Game::pickRandomSpawn(%team);
   else
   {
      %spawn = Game::pickStartSpawn(%team);
      if(%spawn == -1)
         return Game::pickRandomSpawn(%team);
      return %spawn;
   }
}

function Game::pickObserverSpawn(%client)
{
   %group = nameToID("MissionGroup\\ObserverDropPoints");
   %count = Group::objectCount(%group);
   if(%group == -1 || !%count)
      %group = nameToID("MissionGroup\\Teams\\team" @ Client::getTeam(%client) @ "\\DropPoints\\Random");
   %count = Group::objectCount(%group);
   if(%group == -1 || !%count)
      %group = nameToID("MissionGroup\\Teams\\team0\\DropPoints\\Random");
   %count = Group::objectCount(%group);
   if(%group == -1 || !%count)
      return -1;
   %spawnIdx = %client.lastObserverSpawn + 1;
   if(%spawnIdx >= %count)
      %spawnIdx = 0;
   %client.lastObserverSpawn = %spawnIdx;
    return Group::getObject(%group, %spawnIdx);
}

function UpdateClientTimes(%time)
{
  for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
    remoteEval(%cl, "setTime", -%time);
}

function Game::notifyMatchStart(%time)
{
   messageAll(1, "Match starts in " @ %time @ " seconds.~wmine_act.wav");
   UpdateClientTimes(%time);
}

function Game::startMatch()
{
    Game::resetScores();
    
    $FFATourney = false;
    $matchStarted = true;
    $missionStartTime = getSimTime();
    Game::checkTimeLimit();
    
    $FlagIsDropped[0] = 0;
    $FlagIsDropped[1] = 0;
    $freeze::FlagClient[0] = 0;
    $freeze::FlagClient[1] = 0;
    
    $freeze::OOB[0] = false;
    $freeze::OOB[1] = false;
    $NoFlagThrow = false;
    $curTimeAdjust = false;
    $countdownStarted = false;
    
    if($Server::BalancedMode && $Server::Half == 1) {
        messageAll(1, "First half has started. Good luck! ~wmine_act.wav");
        $FFATourney = true;
    }
    else {
        messageAll(1, "Match started.~wmine_act.wav");
        $FFATourney = true;
    }
    
    %numTeams = getNumTeams();
    for(%i = 0; %i < %numTeams; %i = %i + 1)
    {
        if($TeamEnergy[%i] != "Infinite")
            $TeamEnergy[%i] = "Infinite";

        $Stats::FlagLoc[%i] = "home";
    }

    for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
    {
        if(%cl.observerMode == "pregame")
        {
            %cl.observerMode = "";
            Client::setControlObject(%cl, Client::getOwnedObject(%cl));
        }

        %cl.lastActiveTimestamp = getSimTime();

        Game::refreshClientScore(%cl);
    }
    
    //send score updatez for team 0/1
    zadmin::ActiveMessage::All(TeamScore, 0, 0);
    zadmin::ActiveMessage::All(TeamScore, 1, 0);
    zadmin::ActiveMessage::All(MatchStarted);
    zadmin::AFKDaemon();
    
    //begin checking player positions for body blocks
    //$BodyBlock::Init = false;
    Game::BodyBlockCheck();
}

// Kinda like startMatch, but without resetting scores.
function Game::startHalf()
{
    
    if (!$Server::BalancedMode)
        return;

    $missionStartTime = getSimTime();
    Game::checkTimeLimit();
    
    $Server::Halftime = false;
    $FFATourney = true;
    $matchStarted = true;
    $TwoMinWarning = false;
    $curTimeAdjust = false;
    $countdownStarted = false;
    
    $FlagIsDropped[0] = 0;
    $FlagIsDropped[1] = 0;
    $freeze::FlagClient[0] = 0;
    $freeze::FlagClient[1] = 0;
    $freeze::OOB[0] = false;
    $freeze::OOB[1] = false;
    
    $NoFlagThrow = false;
    
    $missionStartTime = getSimTime();
    Game::checkTimeLimit();
    
  messageAll(1, "Second half has started!~wmine_act.wav");
  
  %numTeams = getNumTeams();
  for(%i = 0; %i < %numTeams; %i = %i + 1)
  {
    $Stats::FlagLoc[%i] = "home";
  }

  for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
  {
    if(%cl.observerMode == "pregame")
    {
      %cl.observerMode = "";
      Client::setControlObject(%cl, Client::getOwnedObject(%cl));
    }

    %cl.lastActiveTimestamp = getSimTime();

    Game::refreshClientScore(%cl);
  }
  
  Game::UpdateClientScores();
  
  zadmin::AFKDaemon();
}

function Game::pickPlayerSpawn(%clientId, %respawn)
{
    
    
   return Game::pickTeamSpawn(Client::getTeam(%clientId), %respawn);
}

function Game::playerSpawn(%clientId, %respawn)
{
    if($NoFlagThrow) { return false; }
    
  if(!$ghosting)
    return false;

    Client::clearItemShopping(%clientId);
    %clientId.observerMode = "";
  %spawnMarker = Game::pickPlayerSpawn(%clientId, %respawn);
  if(!%respawn)
  {
    // initial drop
    bottomprint(%clientId, "<jc><f0>Mission: <f1>" @ $missionName @ "   <f0>Mission Type: <f1>" @ $Game::missionType @ "\n<f0>Press <f1>'O'<f0> for specific objectives.", 5);
  }
    if(%spawnMarker) {
        %clientId.guiLock = "";
        %clientId.dead = "";
      if(%spawnMarker == -1)
      {
        %spawnPos = "0 0 300";
        %spawnRot = "0 0 0";
      }
      else
      {
        %spawnPos = GameBase::getPosition(%spawnMarker);
        %spawnRot = GameBase::getRotation(%spawnMarker);
      }

        if(!String::ICompare(Client::getGender(%clientId), "Male"))
        %armor = "larmor";
      else
        %armor = "lfemale";

      %pl = spawnPlayer(%armor, %spawnPos, %spawnRot);
      echo("SPAWN: \"" @ Client::getName(%clientID) @ "\": cl:" @ %clientId @ " pl:" @ %pl @ " marker:" @ %spawnMarker @ " armor:" @ %armor);
      if(%pl != -1)
      {
        GameBase::setTeam(%pl, Client::getTeam(%clientId));
        Client::setOwnedObject(%clientId, %pl);
        Game::playerSpawned(%pl, %clientId, %armor, %respawn);

        if($matchStarted)
          Client::setControlObject(%clientId, %pl);
        else
        {
          %clientId.observerMode = "pregame";
          Client::setControlObject(%clientId, Client::getObserverCamera(%clientId));
          Observer::setOrbitObject(%clientId, %pl, 3, 3, 3);
        }
      }
    return true;
    }
    else {
        Client::sendMessage(%clientId,0,"Sorry No Respawn Positions Are Empty - Try again later ");
      return false;
    }
}

function Game::playerSpawned(%pl, %clientId, %armor)
{
    %clientId.spawn = 1;
    %max = getNumItems();

    for(%i = 0; (%item = $spawnBuyList[%i]) != ""; %i++)
    {
        buyItem(%clientId,%item);
        if(%item.className == Weapon)
            %clientId.spawnWeapon = %item;
    }

    %clientId.spawn = "";

    if(%clientId.spawnWeapon != "")
    {
        Player::useItem(%pl,%clientId.spawnWeapon);
        %clientId.spawnWeapon = "";
    }
    
    $PlayerHasSpawned[%clientId] = true;
}

function Game::autoRespawn(%client)
{
    if(%client.dead == 1)
        Game::playerSpawn(%client, "true");
}

function onServerGhostAlwaysDone()
{
}

    function Game::initialMissionDrop(%clientId) {

        Client::setGuiMode(%clientId, $GuiModePlay);

        if($Server::TourneyMode) {
            GameBase::setTeam(%clientId, -1);
        }
        else {
            if(%clientId.observerMode == "observerFly" || %clientId.observerMode == "observerOrbit" || %clientId.observerMode == "observerFirst") {
                %clientId.observerMode = "observerOrbit";
                %clientId.guiLock = "";
                Observer::jump(%clientId);
                return;
            }

            %numTeams = getNumTeams();
            %curTeam = Client::getTeam(%clientId);

            if(%curTeam >= %numTeams || (%curTeam == -1 && (%numTeams < 2 || $Server::AutoAssignTeams)) ) {
                Game::assignClientTeam(%clientId);
            }
        }

        Client::setControlObject(%clientId, Client::getObserverCamera(%clientId));
        %camSpawn = Game::pickObserverSpawn(%clientId);
        Observer::setFlyMode(%clientId, GameBase::getPosition(%camSpawn),
        GameBase::getRotation(%camSpawn), true, true);

        if(Client::getTeam(%clientId) == -1) {
            %clientId.observerMode = "pickingTeam";
            if(($Server::TourneyMode) && ($matchStarted || $matchStarting)) {
                %clientId.observerMode = "observerFly";
                return;
            }
            else if($Server::TourneyMode) {

                if($Server::TeamDamageScale) {
                    %td = "ENABLED";
                }
                else {
                    %td = "DISABLED";
                }

                if ($Server::BalancedMode) {
                    %mode = "Balanced ";
                    bottomprint(%clientId, "<jc><f1>Server is running in " @ %mode @ "Competition Mode\nPick a team.\nTeam damage is " @ %td, 0);
                }
            }

            Client::buildMenu(%clientId, "Pick a team:", "InitialPickTeam");
            Client::addMenuItem(%clientId, "0Observe", -2);
            //re-add Automatic
            Client::addMenuItem(%clientId, "1Automatic", -1);
            
                for(%i = 0; %i < getNumTeams(); %i = %i + 1){
                    Client::addMenuItem(%clientId, (%i+2) @ getTeamName(%i), %i);
                    %clientId.justConnected = "";
                }
                
        }
        else {
            Client::setSkin(%clientId, $Server::teamSkin[Client::getTeam(%clientId)]);
            if(%clientId.justConnected) {
                centerprint(%clientId, $Server::JoinMOTD, 0);
                %clientId.observerMode = "justJoined";
                %clientId.justConnected = "";
            }
            else if(%clientId.observerMode == "justJoined") {
                centerprint(%clientId, "");
                %clientId.observerMode = "";
                Game::playerSpawn(%clientId, false);
            }
            else {
                Game::playerSpawn(%clientId, false);
            }
        }

        if($TeamEnergy[Client::getTeam(%clientId)] != "Infinite") {
            $TeamEnergy[Client::getTeam(%clientId)] += $InitialPlayerEnergy;
            %clientId.teamEnergy = 0;
        }
    }

function processMenuInitialPickTeam(%clientId, %team)
{
  if($Server::TourneyMode && $matchStarted)
    %team = -2;

  if(%team == -2)
  {
    Observer::enterObserverMode(%clientId);
  }
  if(%team == -1)
  {
    Game::assignClientTeam(%clientId);
    %team = Client::getTeam(%clientId);
  }
  if(%team != -2)
  {
    GameBase::setTeam(%clientId, %team);
    if($TeamEnergy[%team] != "Infinite")
            $TeamEnergy[%team] += $InitialPlayerEnergy;
    %clientId.teamEnergy = 0;
    Client::setControlObject(%clientId, -1);
    Game::playerSpawn(%clientId, false);
  }
  if($Server::TourneyMode && !$CountdownStarted)
  {
    if(%team != -2)
    {
      Game::DisplayReadyMessage(%clientId);
      // bottomprint(%clientId, "<f1><jc>Press FIRE when ready.", 0);
      %clientId.notready = true;
      %clientId.notreadyCount = "";
    }
    else
    {
      bottomprint(%clientId, "", 0);
      %clientId.notready = "";
      %clientId.notreadyCount = "";
    }
  }
  //$BodyBlock::Init = false;
}

function Game::ForceTourneyMatchStart()
{
  %playerCount = 0;
  for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
  {
    if(%cl.observerMode == "pregame")
      %playerCount++;
  }
  if(%playerCount == 0)
    return;

  for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
  {
    if(%cl.observerMode == "pickingTeam")
      processMenuInitialPickTeam(%cl, -2); // throw these guys into observer
    for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
    {
      %cl.notready = "";
      %cl.notreadyCount = "";
      bottomprint(%cl, "", 0);
    }
  }
  Server::Countdown(30);
}

function Game::CheckTourneyMatchStart()
{
   if($CountdownStarted || $matchStarted)
      return;

   // loop through all the clients and see if any are still notready
   %playerCount = 0;
   %notReadyCount = 0;
   $FFATourney = true;

   for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
   {
      if(%cl.observerMode == "pickingTeam")
      {
         %notReady[%notReadyCount] = %cl;
         %notReadyCount++;
      }
      else if(%cl.observerMode == "pregame")
      {
         if(%cl.notready)
         {
            %notReady[%notReadyCount] = %cl;
            %notReadyCount++;
         }
         else
            %playerCount++;
      }
   }
   if(%notReadyCount)
   {
      if(%notReadyCount == 1)
         MessageAll(0, Client::getName(%notReady[0]) @ " is holding things up!");
      else if(%notReadyCount < 4)
      {
         for(%i = 0; %i < %notReadyCount - 2; %i++)
            %str = Client::getName(%notReady[%i]) @ ", " @ %str;

         %str = %str @ Client::getName(%notReady[%i]) @ " and " @ Client::getName(%notReady[%i+1])
                     @ " are holding things up!";
         MessageAll(0, %str);
      }
      return;
   }

   if(%playerCount != 0)
   {
      for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
      {
         %cl.notready = "";
         %cl.notreadyCount = "";
         bottomprint(%cl, "", 0);
      }
      Server::Countdown(30);
   }
}

function Game::resetScores(%client)
{
    if(%client == "") {
      for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
    {
        %cl.scoreKills = 0;
      %cl.scoreDeaths = 0;
            %cl.ratio = 0;
      %cl.score = 0;
        }
    }
    else
  {
    %client.scoreKills = 0;
    %client.scoreDeaths = 0;
        %client.ratio = 0;
    %client.score = 0;
    }
}

function remoteSetArmor(%player, %armorType)
{
    if ($ServerCheats) {
        checkMax(Player::getClient(%player),%armorType);
      Player::setArmor(%player, %armorType);
    }
    else if($TestCheats) {
      Player::setArmor(%player, %armorType);
    }
}


function Game::onPlayerConnected(%playerId)
{
   //afk monitor
   %playerId.lastActiveTimestamp = getSimTime();

   %playerId.scoreKills = 0;
   %playerId.scoreDeaths = 0;
    %playerId.score = 0;
   %playerId.justConnected = true;
   $menuMode[%playerId] = "None";
   Game::refreshClientScore(%playerId);
}

function Game::assignClientTeam(%playerId)
{
    if($teamplay)
    {
        %name = Client::getName(%playerId);
        %numTeams = getNumTeams();

        if($teamPreset[%name] != "")
        {
            if($teamPreset[%name] < %numTeams)
            {
                GameBase::setTeam(%playerId, $teamPreset[%name]);
                echo(Client::getName(%playerId), " was preset to team ", $teamPreset[%name]);
                return;
            }
        }

        %numPlayers = getNumClients();

        for(%i = 0; %i < %numTeams; %i = %i + 1)
            %numTeamPlayers[%i] = 0;

        for(%i = 0; %i < %numPlayers; %i = %i + 1)
        {
            %pl = getClientByIndex(%i);
            if(%pl != %playerId)
            {
                %team = Client::getTeam(%pl);
                %numTeamPlayers[%team] = %numTeamPlayers[%team] + 1;
            }
        }

        %leastPlayers = %numTeamPlayers[0];
        %leastTeam = 0;

        for(%i = 1; %i < %numTeams; %i = %i + 1)
        {
            if  ( (%numTeamPlayers[%i] < %leastPlayers) ||
                ( (%numTeamPlayers[%i] == %leastPlayers) &&
                ($teamScore[%i] < $teamScore[%leastTeam] ) ))
            {
                %leastTeam = %i;
                %leastPlayers = %numTeamPlayers;
            }
        }

        GameBase::setTeam(%playerId, %leastTeam);
        echo(Client::getName(%playerId), " was automatically assigned to team ", %leastTeam);
    }
    else
    {
        GameBase::setTeam(%playerId, 0);
    }
}

function Client::onKilled(%playerId, %killerId, %damageType)
{

    if($NoFlagThrow) { return; }

    echo("GAME: kill "@%killerId@" "@%playerId@" " @ %damageType);

    %playerId.guiLock = true;

    Client::setGuiMode(%playerId, $GuiModePlay);
    if(!String::ICompare(Client::getGender(%playerId), "Male"))
    {
        %playerGender = "his";
    }
    else
    {
        %playerGender = "her";
    }

    %ridx = floor(getRandom() * ($numDeathMsgs - 0.01));
    %victimName = Client::getName(%playerId);

    if (!%killerId)
    {
        //turret
        %damageType = $EnergyDamageType;

        messageAll(0, strcat(%victimName, " dies."), $DeathMessageMask);
    }
    else if (%killerId == %playerId)
    {
        //suicide
        %playerCratered = false;
        if(%damageType == $LandingDamageType) { %playerCratered = true; }
        
        %damageType = $SuicideDamageType;

        %oopsMsg = sprintf($deathMsg[-2, %ridx], %victimName, %playerGender);
        messageAll(0, %oopsMsg, $DeathMessageMask);

        //score
        Client::adjustScore(%killerId, "Suicide");
        %playerId.scoreDeaths++;
        %playerId.Deaths++;
        
        
        //LETS SET A GLOBAL COUNTER FOR SUICIDES AND BUILD A RATE THAT IS ACCURATE FOR CHECKING TIME
        //THIS IS BECAUSE SUICIDES ARE A GIVEN IN THE WORLD OF TRIBE
        
        %suicideClients = 0;

        for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
            
            if ( (Client::getTeam(%cl) == 0) || (Client::getTeam(%cl) == 1) ) {
                
                //if players are on a team add to the count
                %suicideClients++;
            }
            else {
                //
                //if observer, do not add to count
                //
            }
        }
        
        %suicideCount = (floor($SuicideMult * %suicideClients) + 1);
        
        if (!$TwoMinWarning && !$curTimeAdjust) {
            
            $SuicideTimeChecker++;
            if ($SuicideTimeChecker >= %suicideCount) {
                //messageAll(0, "Suicide Time Check!");
                Game::checkTimeLimit();
                $SuicideTimeChecker = 0;
            }
        }
        else {
            //reset tracker
            $SuicideTimeChecker = 0;
        }
    }
    else
    {
        if(!String::ICompare(Client::getGender(%killerId), "Male"))
            %killerGender = "his";
        else
            %killerGender = "her";

        if($teamplay && (Client::getTeam(%killerId) == Client::getTeam(%playerId)))
        {
            if(%damageType != $MineDamageType)
            {
                messageAll(0, strcat(Client::getName(%killerId),
                            " mows down ", %killerGender, " teammate, ", %victimName), $DeathMessageMask);
                %damageType = $TeamkillDamageType;

                Client::adjustScore(%killerId, "TeamKill");
            }
            else
            {
                messageAll(0, strcat(Client::getName(%killerId),
                            " killed ", %killerGender, " teammate, ", %victimName ," with a mine."), $DeathMessageMask);
                %damageType = $MineTeamkillDamageType;

                Client::adjustScore(%killerId, "MineTeamKill");
            }

            Game::refreshClientScore(%killerId);
        }
        else
        {
            //%killerid.kills[ $zadmin::WeaponName[%damageType] ]++;
            //%playerid.deaths[ $zadmin::WeaponName[%damageType] ]++;

            %killerid.kills++;
            %playerid.deaths++;
            %killerId.scoreKills++;
            %playerId.scoreDeaths++;  // test play mode

            %obitMsg = sprintf($deathMsg[%damageType, %ridx], Client::getName(%killerId), %victimName, %killerGender, %playerGender);
            messageAll(0, %obitMsg, $DeathMessageMask);

            if (Client::getName(%killerId) != "")
            {
                //%killerId.score++;
                Client::adjustScoreNoUpdate(%killerId, $zadmin::WeaponName[%damageType]);

                %time = getIntegerTime(true) >> 5;
                %oppositeTeam = Client::GetTeam(%playerId) ^ 1;

                if ( (%time == $Stats::FlagDropped[%oppositeTeam]) && ($Stats::PlayerDropped[%oppositeTeam] == %playerId) ) {
                    Client::adjustScoreNoUpdate(%killerId, "CarrierKill");
                }

                if ($zadmin::WeaponName[%damageType] == "Mortar")
                    Client::adjustScoreNoUpdate(%playerId, "MortarDeath");
                else
                    Client::adjustScoreNoUpdate(%playerId, "OtherDeath");
            }

            Client::refreshScore(%playerId);
            Client::refreshScore(%killerId);
        }
    }
    
    //Body block conditions to set when dead
    $BodyBlock::Speed[%playerId] = 0;
    $PlayerHasSpawned[%playerId] = false;

    Game::clientKilled(%playerId, %killerId);
    
    //collect stat of how many times player craters into the ground
    if (%playerCratered) {
        zadmin::ActiveMessage::All(KillTrak, %killerId, %playerId, "Landing");
    }
    else {
        zadmin::ActiveMessage::All(KillTrak, %killerId, %playerId, $zadmin::WeaponName[%damageType]);
    }

    %now = getSimTime();
    %killerId.lastActiveTimestamp = %now;
    %playerId.lastActiveTimestamp = %now;

}

function Game::clientKilled(%playerId, %killerId)
{
   // do nothing
}

function Client::leaveGame(%clientId)
{
   // do nothing
}

function Player::enterMissionArea(%player)
{
   echo("Player " @ %player @ " entered the mission area.");
}

function Player::leaveMissionArea(%player)
{
   echo("Player " @ %player @ " left the mission area.");
}

function GameBase::getHeatFactor(%this)
{
   return 0.0;
}

function Game::NextHalf()
{
    if (!$Server::BalancedMode)
        return;
    
    Game::SwapScores();
    Game::SwapTeams();

    for (%i = 0; %i < getNumTeams(); %i++) {
      Flag::ResetFlag($teamFlag[%i]);
      $FlagIsDropped[%i] = false;
    }

    // Just to change the time on client HUDs
    UpdateClientTimes($Server::TimeLimit * 60);

    MessageAll(0, "~wcapturedtower.wav");
    
    if ($Server::TourneyMode) {
        //only display scoreboard if tourney mode
        
        Game::DisplayHalfScoreboard();
    }
    else {
        //start match
        Game::startHalf();
    }
    ObjectiveMission::refreshTeamScores();
}

function Game::ResetHalf() {
    //back to beginning of match
    $Server::Half = 1;
    $Server::Halftime = false;
    $firstHalfCapped = false;
}

function Game::HalfTimeNow() {

    $Server::Halftime = true;
    $Server::Half = 2;
    MessageAll(0, "***** HALF TIME *****~wshieldhit.wav");
    
    $Server::FirstHalfTime = ((getSimTime() - $missionStartTime)/60);
    $firstTimeHalf = $Server::FirstHalfTime;
    //MessageAll(0, "FirstHalfDuration: " @ $firstTimeHalf);
    
    if ($Server::BalancedMode == 1 && ($firstTimeHalf % 60) == 0) {
        $halftimeMins = $firstTimeHalf;
        $halftimeSecs = 0;
    }
    else if (!$firstHalfCapped) {
        $halftimeMins = $Server::timeLimit;
        $halftimeSecs = 0;
    }
    else {
        $halftimeMins = floor($firstTimeHalf);
        $halftimeSecs = floor(($firstTimeHalf - $halftimeMins)*60);
    }
    
    $matchStarted = false;
    $countdownStarted = false;
    $timeCheckSwitch = false;
    
    schedule('Server::GameTimeBalance();', 3);
    schedule('Game::NextHalf();', 3);
}

function Game::DisplayReadyMessage(%client)
{
  if ($Server::BalancedMode)
  {
    %scoreLimit = $teamScoreLimit * $Server::Half - $Server::Half;
    %scoreboard = "<jc>" @ "<f1>The game is set to <f2>BALANCED MODE\n" @
                  "<f1>Teams will switch sides at <f2>" @ %scoreLimit @ " <f1>total caps\n";
                  //"<f1>First team to <f2>" @ $teamScoreLimit @ " <f1>wins\n\n";
    %scoreboard = %scoreboard @ "<f1>Press FIRE when ready.";
    CenterPrint(%client, %scoreboard, 0);
  }
  else
  {
    //bottomprint(%client, "<f1><jc>Press FIRE when ready.", 0);
    CenterPrint(%client, "<f1><jc>Press FIRE when ready.", 0);
  }
}

function Game::DisplayHalfScoreboard()
{
    if ($Server::BalancedMode == 1) {
        %scoreLimit = $teamScoreLimit * $Server::Half - $Server::Half;
        %scoreboard = "<jc>" @ "<f1>First half duration: <f2>" @ $halftimeMins @ " <f1>minutes <f2>" @ $halftimeSecs @ " <f1>seconds.\n\n" @
                "<f2>Scores at halftime:\n" @
                "<f1>" @ getTeamName(0) @ ": <f2>" @ $teamScore[0] @ "\n" @
                "<f1>" @ getTeamName(1) @ ": <f2>" @ $teamScore[1] @ "\n\n";
                //"<f1> First team to <f2>" @ $teamScoreLimit @ " <f1>wins.\n\n";
        if ($Server::Half == 2) {
            %scoreboard = %scoreboard @ "<f1>Match forces in 5 seconds. Please stand by...";
            schedule('Game::ForceTourneyMatchStart();', 5);
        }
    }
    else if ($Server::BalancedMode == 2) {
        %scoreLimit = $teamScoreLimit * $Server::Half - $Server::Half;
        %scoreboard = "<jc>" @ "<f1>First half duration: <f2>" @ $halftimeMins @ " <f1>minutes <f2>" @ $halftimeSecs @ " <f1>seconds.\n\n" @
                "<f2>Scores at halftime:\n" @
                "<f1>" @ getTeamName(0) @ ": <f2>" @ $teamScore[0] @ "\n" @
                "<f1>" @ getTeamName(1) @ ": <f2>" @ $teamScore[1] @ "\n\n";
                //"<f1> First team to <f2>" @ %scoreLimit @ " <f1>total caps wins.\n\n";
        if ($Server::Half == 2) {
            %scoreboard = %scoreboard @ "<f1>Match forces in 5 seconds. Please stand by...";
            schedule('Game::ForceTourneyMatchStart();', 5);
        }
    }
    else {
        return;
    }
    for (%i = 0; %i < getNumClients(); %i++) { CenterPrint(getClientByIndex(%i), %scoreboard, 0); }
}

function Game::SwapScores()
{
  %tempScore = $teamScore[0];
  $teamScore[0] = $teamScore[1];
  $teamScore[1] = %tempScore;

  Game::UpdateClientScores();
}

function Game::UpdateClientScores()
{
  for (%i = 0; %i < getNumClients(); %i++)
  {
    %cl = getClientByIndex(%i);
    zadmin::ActiveMessage::Single(%cl, TeamScore, 0, $TeamScore[0]);
    zadmin::ActiveMessage::Single(%cl, TeamScore, 1, $TeamScore[1]);
  }
}

function Game::SwapTeams()
{
  %numClients = getNumClients();
  for(%i = 0; %i < %numClients; %i++)
  {
    %clientId = getClientByIndex(%i);
    Game::SwapPlayer(%clientId);
  }
}

function Game::SwapPlayer(%clientId)
{
  %clTeam = Client::GetTeam(%clientId);
  if (%clTeam == -1)  // Observer
    return;
  else if (%clTeam == 0)
    %clNewTeam = 1;
  else if (%clTeam == 1)
    %clNewTeam = 0;
  else
    return;

  %player = Client::getOwnedObject(%clientId);

  if(%player != -1 && getObjectType(%player) == "Player" && !Player::isDead(%player))
  {
    playNextAnim(%clientId);
    Player::kill(%clientId);
  }
  %clientId.observerMode = "";

  //messageAll(0, Client::getName(%clientId) @ " was switched to " @ getTeamName(%clNewTeam));

  GameBase::setTeam(%clientId, %clNewTeam);
  %clientId.teamEnergy = 0;
  Client::clearItemShopping(%clientId);
  if(Client::getGuiMode(%clientId) != 1)
    Client::setGuiMode(%clientId,1);
  Client::setControlObject(%clientId, -1);

  Game::playerSpawn(%clientId, false);
  %team = Client::getTeam(%clientId);
  if($TeamEnergy[%team] != "Infinite")
    $TeamEnergy[%team] += $InitialPlayerEnergy;

  if($Server::TourneyMode && !$CountdownStarted)
  {
      
    //Game::DisplayReadyMessage(%clientId);
    
    // bottomprint(%clientId, "<f1><jc>Press FIRE when ready.", 0);
    %clientId.notready = true;
  }
  
}

function Game::BodyBlockCheck()
{
    
    if(!$BodyBlock::Enabled) { return; }
    if($loadingMission) { return; }
    
    //if we in a pause, back out and try again in 1 second
    if ($freezedata::actice == 1) {
        schedule("Game::BodyBlockCheck();", 1);
        return;
    }

    //set bodyblock init to false in the admin menu team change of any kind
    
    //if(!$BodyBlock::Init) {
        //$BodyBlock::Init = true;
        //$BBClient::Count = 0;
        //for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {

            //$BBClient::Player[$BBClient::Count] = %cl;
            //$BBClient::Count++;

        //}
    //}
    //only look for player speed if the match has begun
    if ($matchStarted) {
        
        //for(%i=0; %i < $BBClient::Count; %i++) {
            
            //%cl = $BBClient::Player[%i];
            
            //if (!$BodyBlock::Calculate[%cl]) {
                //if ($PlayerHasSpawned[%cl]) {

                    //$BodyBlock::Speed[%cl] = Game::getPlayerSpeed(%cl);
                    
                //}
            //}
        //}
        
        //cycle through clients
        for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
            //check to see if we are in the process of calculating a BB first
            if (!$BodyBlock::Calculate[%cl]) {
                //player must be spawned in
                if ($PlayerHasSpawned[%cl]) {
                    %clTeam = Client::getTeam(%cl);
                    if (%clTeam == 0 || %clTeam == 1) {
                        $BodyBlock::Speed[%cl] = Game::getPlayerSpeed(%cl);
                    }
                }
            }
        }
    }
    schedule("Game::BodyBlockCheck();", 1);
}

function Game::getPlayerSpeed(%cl)
{
    %playerVelocity = Item::getVelocity(%cl);
    %playerSpeed = Vector::getDistance(%playerVelocity, "0 0 0");
    return %playerSpeed;
}

function Game::distanceToFlag(%cl, %flagTeam, %isFlag)
{
    //position of client (or flag if we are comparing to stand)
    %clPos = GameBase::getPosition( %cl );
    
    //position of flag
    if (%isFlag) {
        %flagPos = GameBase::getPosition( $teamFlag[%flagTeam] );
    }
    //otherwise position of stand
    else {
        %flagPos = $teamFlagStandPos[%flagTeam];
    }
    
    %clPosX = getWord(%clPos, 0);
    %clPosY = getWord(%clPos, 1);
    %clPosZ = getWord(%clPos, 2);
    
    %flagPosX = getWord(%flagPos, 0);
    %flagPosY = getWord(%flagPos, 1);
    %flagPosZ = getWord(%flagPos, 2);
    
    %distanceToFlag = Vector::getDistance(%clPosX @ " " @ %clPosY @ " " @ %clPosZ, %flagPosX @ " " @ %flagPosY @ " " @ %flagPosZ);

    return %distanceToFlag;
}
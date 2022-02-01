exec("code.game.cs");
$flagReturnTime = 45;
$Game::timeClockUpdate = 20;

$Game::EgrabRadius = 30;
$Game::ClutchReturnRadius = 15;

function ObjectiveMission::missionComplete()
{

  if ($Server::BalancedMode && $Server::Half == 1) {
      
    Server::nextMission();
    return;
  }
    
  MessageAll(1, "***** GAME OVER *****~wshieldhit.wav");
  
  $missionComplete = true;
  zadmin::ActiveMessage::All( MissionComplete );
  %group = nameToID("MissionCleanup/ObjectivesSet");
  for(%i = 0; (%obj = Group::getObject(%group, %i)) != -1; %i++)
  {
    ObjectiveMission::objectiveChanged(%obj);
  }
  for(%i = 0; %i < getNumTeams(); %i++) {
    Team::setObjective(%i, $firstObjectiveLine-4, " ");
    Team::setObjective(%i, $firstObjectiveLine-3, "<f5>Mission Summary:");
    Team::setObjective(%i, $firstObjectiveLine-2, " ");
    
  }
    
  ObjectiveMission::setObjectiveHeading();
  ObjectiveMission::refreshTeamScores();
  %lineNum = "";
  $missionComplete = false;

  // back out of all the functions...
  schedule("Server::nextMission();", 0);
}

function ObjectiveMission::setObjectiveHeading()
{
  if($missionComplete)
  {
    %curLeader = 0;
    %tieGame = false;
    %tie = 0;
    %tieTeams[%tie] = %curLeader;
    for(%i = 0; %i < getNumTeams() ; %i++)
      echo("GAME: teamfinalscore " @ %i @ " " @ $teamScore[%i]);

    for(%i = 1; %i < getNumTeams() ; %i++)
    {
      if($teamScore[%i] == $teamScore[%curLeader]) {
        %tieGame = true;
        %tieTeams[%tie++] = %i;
      }
      else if($teamScore[%i] > $teamScore[%curLeader])
      {
        %curLeader = %i;
        %tieGame = false;
        %tie = 0;
        %tieTeams[%tie] = %curLeader;
      }
    }
    if(%tieGame) {
      for(%g = 0; %g <= %tie; %g++) {
        %names = %names @ getTeamName(%tieTeams[%g]);
        if(%g == %tie-1)
          %names = %names @ " and ";
        else if(%g != %tie)
          %names = %names @ ", ";
      }
    if(%tie > 1)
      %names = %names @ " all";
    }
    for(%i = -1; %i < getNumTeams(); %i++)
    {
      objective::displayBitmap(%i,0);
      if(!%tieGame) {
        if(%i == %curLeader) {
          if($teamScore[%curLeader] == 1)
            Team::setObjective(%i, 1, "<F5>           Your team won the mission with " @ $teamScore[%curLeader] @ " point!");
          else
            Team::setObjective(%i, 1, "<F5>           Your team won the mission with " @ $teamScore[%curLeader] @ " points!");
        }
        else {
          if($teamScore[%curLeader] == 1)
            Team::setObjective(%i, 1, "<F5>     The " @ getTeamName(%curLeader) @ " team won the mission with " @ $teamScore[%curLeader] @ " point!");
          else
            Team::setObjective(%i, 1, "<F5>     The " @ getTeamName(%curLeader) @ " team won the mission with " @ $teamScore[%curLeader] @ " points!");
        }
      }
      else {
        if(getNumTeams() > 2) {
          Team::setObjective(%i, 1, "<F5>     The " @ %names @ " tied with a score of " @ $teamScore[%curLeader]);
        }
        else
          Team::setObjective(%i, 1, "<F5>     The mission ended in a tie where each team had a score of " @ $teamScore[%curLeader]);
      }
      Team::setObjective(%i, 2, " ");
    }
  }
  else {
    for(%i = -1; %i < getNumTeams(); %i++)
    {
      objective::displayBitmap(%i,0);
      Team::setObjective(%i,1, "<f5>Mission Completion:");
      Team::setObjective(%i, 2,"<f1>   - " @ $teamScoreLimit @ " points needed to win the mission.");
    }
  }
  if(!$Server::timeLimit)
    %str = "<f1>   - No time limit on the game.";
  else if($timeLimitReached)
    %str = "<f1>   - Time limit reached.";
  else if($missionComplete)
  {
    %time = getSimTime() - $missionStartTime;
    %minutes = Time::getMinutes(%time);
    %seconds = Time::getSeconds(%time);
    if(%minutes < 10)
      %minutes = "0" @ %minutes;
    if(%seconds < 10)
      %seconds = "0" @ %seconds;
    %str = "<f1>   - Total match time: " @ %minutes @ ":" @ %seconds;
  }
  else
    %str = "<f1>   - Time remaining: " @ floor($Server::timeLimit - (getSimTime() - $missionStartTime) / 60) @ " minutes.";


  for(%i = -1; %i < getNumTeams(); %i++) {
    Team::setObjective(%i, 3, " ");
    Team::setObjective(%i, 4, "<f5>Mission Information:");
    Team::setObjective(%i, 5, "<f1>   - Mission Name: " @ $missionName);
    Team::setObjective(%i, 6, %str);
  }

}

function objective::displayBitmap(%team, %line)
{
 if($TestMissionType == "CTF") {
  %bitmap1 = "capturetheflag1.bmp";
  %bitmap2 = "capturetheflag2.bmp";
 }
 if(%bitmap1 == "" || %bitmap2 == "")
   Team::setObjective(%team, %line, " ");
 else
   Team::setObjective(%team, %line, "<jc><B0,0:" @ %bitmap1 @ "><B0,0:" @ %bitmap2 @ ">");
}


function Game::CollectDamage()
{
    
    if(!$Collector::DamageEnabled)
        return;
    
    for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
        
        %clTeam = Client::getTeam(%cl);
        if(%clTeam == 0 || %clTeam == 1) {
            
            zadmin::ActiveMessage::All( DamageDealt, %cl, 0, $DmgTracker::DamageOut[%cl] );
            zadmin::ActiveMessage::All( TeamDamageDealt, %cl, 0, $DmgTracker::TeamDamageOut[%cl] );
            $DmgTracker::DamageOut[%cl] = 0;
            $DmgTracker::TeamDamageOut[%cl] = 0;
            zadmin::ActiveMessage::All( DamageDealt, 0, %cl, $DmgTracker::DamageIn[%cl] );
            zadmin::ActiveMessage::All( TeamDamageDealt, 0, %cl, $DmgTracker::TeamDamageIn[%cl] );
            $DmgTracker::DamageIn[%cl] = 0;
            $DmgTracker::TeamDamageIn[%cl] = 0;
        }
    }
    
}

function Game::checkTimeLimit()
{

  $timeLimitReached = false;
  ObjectiveMission::setObjectiveHeading();
  
  if($Server::timeLimit == 0 || $freezedata::actice) { return; }
  
   // IF BEGINNING TIME CHECK BELOW DOES NOT YIELD A WHOLE NUMBER VALUE, WE NEED TO ADJUST $MISSIONTIME ACCORDINGLY
  // SO IF remainder val < 0.5 and val > 0, floor(val).... else floor(val+1)
  // WE ARE STILL GOING TO CHECK IF MOD BY TIME UPDATE INTERVAL NEEDS TO HAPPEN
  
  $simTimeTemp = getSimTime();
  %curTimeLeft = ($Server::timeLimit * 60) + ($missionStartTime - $simTimeTemp);
  $lastKnownCurTime = %curTimeLeft;
  %curTimeRemainder = (%curTimeLeft - floor(%curTimeLeft));
  
  //MessageAll(0, "SimTimeTemp: " @ $simTimeTemp);
  //MessageAll(0, "MissionStartTime: " @ $missionStartTime);
  //MessageAll(0, "curTimeLeft: " @ %curTimeLeft);
  //MessageAll(0, "curTimeLeftFLOOR: " @ floor(%curTimeLeft));
  //MessageAll(0, "curTimeRemainder: " @ %curTimeRemainder);
  

  if (%curTimeRemainder == 0) {
      //WHOLE NUMBER GOOD, DO NOTHING
  }
  else {
      //MessageAll(0, "curTimeBeforeAdj: " @ %curTimeLeft);
      //FLOAT NUMBER, BAD... NEED TO DO STUFF NOW
      if (%curTimeRemainder < 0.499999) {
          //ROUND DOWN
          //$missionStartTime -= %curTimeRemainder;
          %curTimeLeft = floor(%curTimeLeft);

      }
      else if (%curTimeRemainder >= 0.499999) {
            //ROUND UP
          //$missionStartTime += (1 - %curTimeRemainder);
          %curTimeLeft = (1 + floor(%curTimeLeft));
          
      }
      else {
          //do nothing
      }
      
      //%curTimeLeft = ($Server::timeLimit * 60) + $missionStartTime - $simTimeTemp;
      //MessageAll(0, "curTimeAfterAdj: " @ %curTimeLeft);
      
  }
  
  if (%curTimeLeft >= $Game::timeClockUpdate) {
      //update this until the last update at timeClockUpdate interval
      //notifications will take over for the last 15 seconds
      $Notifications::curTimeLeft = %curTimeLeft;
  }
 
  //now at this stage we will always have a WHOLE number
 
  //MessageAll(0, "checkTimeLimit: " @ %curTimeLeft);
    
  if ($matchStarted)
  {
      
    //collect damage stats in intervals
    Game::CollectDamage();
 
    if(%curTimeLeft <= 0) {
        if(%curTimeLeft < -60) {
            
            echo("GAME: Time limit exceeded.");
            $timeLimitReached = true;
            %set = nameToID("MissionCleanup/ObjectiveSet");
            for(%i = 0; (%obj = Group::getObject(%set, %i)) != -1; %i++)
                GameBase::virtual(%obj, "timeLimitReached", %clientId);
        
            schedule('MessageAll(0, "Server time limit exceeded... Switching to next map...");', 5);
            schedule("ObjectiveMission::missionComplete();", 9);
        }
        else {

            echo("GAME: timelimit");
            $timeLimitReached = true;
            echo("checking for objective time limit status...");
            %set = nameToID("MissionCleanup/ObjectiveSet");
            for(%i = 0; (%obj = Group::getObject(%set, %i)) != -1; %i++)
                GameBase::virtual(%obj, "timeLimitReached", %clientId);

            ObjectiveMission::missionComplete();
        }
    }
    else
    {
        
      //at 2 minute mark switch to old scheduling check
    if (%curTimeLeft <= 120) {
            
        $TwoMinWarning = true;

        //checks to see if we are on the interval path based on timeClockUpdate value
        if((%curTimeLeft % $Game::timeClockUpdate) == 0) {
            
                if(%curTimeLeft >= $Game::timeClockUpdate)
                    schedule("Game::checkTimeLimit();", $Game::timeClockUpdate);
                else
                    schedule("Game::checkTimeLimit();", %curTimeLeft);
                
        }
        else {
                
            //we've fallen off the path, need to adjust now
            
            //might not need to floor here if everything works smoothly above
            %curTimeSched = floor(%curTimeLeft % $Game::timeClockUpdate);
            schedule("Game::checkTimeLimit();", %curTimeSched);
        }
    }
    else {
        //SERVER IS EMPTY, LETS SCHEDULE THE OLD FASHION WAY
        if ($ServerIsEmpty) {
            
            schedule("Game::checkTimeLimit();", $Game::timeClockUpdate);
        }
        //OTHERWISE DO NOTHING HANDLED BY SUICIDE COUNTER NOW
    }
    
    //MessageAll(0, "clientTime: " @ %curTimeLeft);
    UpdateClientTimes(%curTimeLeft);
    }
  }
}

function Game::UpdateTimeOnly() {
    
    %curTimeLeft = ($Server::timeLimit * 60) + ($missionStartTime - $simTimeTemp);
    UpdateClientTimes(%curTimeLeft);

}

function Vote::changeMission()
{
  $missionComplete = true;
  ObjectiveMission::refreshTeamScores();
  %group = nameToID("MissionCleanup/ObjectivesSet");
  %lineNum = "";
  for(%i = 0; (%obj = Group::getObject(%group, %i)) != -1; %i++)
  {
    ObjectiveMission::objectiveChanged(%obj);
  }
  for(%i = 0; %i < getNumTeams(); %i++) {
    Team::setObjective(%i, $firstObjectiveLine-2, " ");
    Team::setObjective(%i, $firstObjectiveLine-1, "<f5>Mission Summary:");
  }
  ObjectiveMission::setObjectiveHeading();
  $missionComplete = false;
}

function ObjectiveMission::checkScoreLimit()
{
  %done = false;
  ObjectiveMission::refreshTeamScores();

  //check for capout win
  for(%i = 0; %i < getNumTeams(); %i++) {
    if($teamScore[%i] >= $teamScoreLimit){
      %done = true;
      break;
    }
  }

  if(%done) {
    ObjectiveMission::missionComplete();
  }
}

function ObjectiveMission::initCheck(%object)
{
 if($TestMissionType == "") {
  %name = gamebase::getdataname(%object);
    if(%name == Flag) {
   if(gamebase::getteam(%object) != -1)
    $TestMissionType = "CTF";
  }
 }

   %object.trainingObjectiveComplete = "";
   %object.objectiveLine = "";
   if(GameBase::virtual(%object, objectiveInit))
      addToSet("MissionCleanup/ObjectivesSet", %object);
}

function Game::refreshClientScore(%clientId)
{
   %team = Client::getTeam(%clientId);
   if(%team == -1) // observers go last.
      %team = 9;
      
   Client::setScore(%clientId, "%n\t%t\t  " @ floor(%clientId.score)  @ "\t%p\t%l", %clientId.score + (9 - %team) * 10000);
}

function ObjectiveMission::refreshTeamScores()
{
   %nt = getNumTeams();
   for (%i=-1; %i<%nt; %i++)
       %teamSize[%i] = "0.001";

 for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
       %teamSize[ Client::getTeam(%cl) ]++;

   Team::setScore(-1, "Obs\t" @ "   " @ 0 @ "\t " @ 0, 0);
   for(%i = 0; %i < %nt; %i++)
   {
      Team::setScore(%i, "%t\t" @ "   " @ $teamScore[%i] @ "\t " @ floor(%teamSize[%i]), 9-%i);
      for(%j = 0; %j < %nt; %j++)
         Team::setObjective(%i,%j+$firstTeamLine, "<f1>   - Team " @ getTeamName(%j) @ " score = " @ $teamScore[%j]);
   }
}

function ObjectiveMission::objectiveChanged(%this)
{
 if(%this.objectiveLine)
      for(%i = -1; %i < getNumTeams(); %i++)
         Team::setObjective(%i,%this.objectiveLine,
            "<f1> " @ GameBase::virtual(%this, getObjectiveString, %i));
}

function Mission::init()
{
  setClientScoreHeading("Player Name\t\x6FTeam\t\xA6Score\t\xD4Ping\t\xF4PL");
  setTeamScoreHeading("Team Name\t\x6fScore\t\xA6Players");

  $firstTeamLine = 7;
  $firstObjectiveLine = $firstTeamLine + getNumTeams() + 1;
  for(%i = -1; %i < getNumTeams(); %i++)
  {
    $teamFlagStand[%i] = "";
    $teamFlag[%i] = "";

    Team::setObjective(%i, $firstTeamLine - 1, " ");
    Team::setObjective(%i, $firstObjectiveLine - 1, " ");
    Team::setObjective(%i, $firstObjectiveLine, "<f5>Mission Objectives: ");
    $firstObjectiveLine++;
    $deltaTeamScore[%i] = 0;
    $teamScore[%i] = 0;
    newObject("TeamDrops" @ %i, SimSet);
    addToSet(MissionCleanup, "TeamDrops" @ %i);
    %dropSet = nameToID("MissionGroup/Teams/Team" @ %i @ "/DropPoints/Random");

    for(%j = 0; (%dropPoint = Group::getObject(%dropSet, %j)) != -1; %j++)
     addToSet("MissionCleanup/TeamDrops" @ %i, %dropPoint);
  }
  $numObjectives = 0;
  newObject(ObjectivesSet, SimSet);
  addToSet(MissionCleanup, ObjectivesSet);

  Group::iterateRecursive(MissionGroup, ObjectiveMission::initCheck);
  %group = nameToID("MissionCleanup/ObjectivesSet");

  ObjectiveMission::setObjectiveHeading();
  for(%i = 0; (%obj = Group::getObject(%group, %i)) != -1; %i++)
  {
    %obj.objectiveLine = %i + $firstObjectiveLine;
    ObjectiveMission::objectiveChanged(%obj);
  }

  ObjectiveMission::refreshTeamScores();
  for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
  {
    %cl.score = 0;
    Game::refreshClientScore(%cl);
  }

  if($TestMissionType == "")
  {

    $TestMissionType = "NONE";
    $NumTowerSwitchs = "";
  }
}

function Game::pickRandomSpawn(%team)
{
   %spawnSet = nameToID("MissionCleanup/TeamDrops" @ %team);
   %spawnCount = Group::objectCount(%spawnSet);
   if(!%spawnCount)
      return -1;
   %spawnIdx = floor(getRandom() * (%spawnCount - 0.1));
   %value = %spawnCount;
 for(%i = %spawnIdx; %i < %value; %i++) {
  %set = newObject("set",SimSet);
  %obj = Group::getObject(%spawnSet, %i);
  if(containerBoxFillSet(%set,$SimPlayerObjectType|$VehicleObjectType,GameBase::getPosition(%obj),2,2,4,0) == 0) {
   deleteObject(%set);
   return %obj;
  }
  if(%i == %spawnCount - 1) {
   %i = -1;
   %value = %spawnIdx;
  }
  deleteObject(%set);
 }
   return false;
}

function Client::leaveGame(%clientId)
{
   echo("GAME: clientdrop " @ %clientId);
   %set = nameToID("MissionCleanup/ObjectivesSet");
   for(%i = 0; (%obj = Group::getObject(%set, %i)) != -1; %i++)
      GameBase::virtual(%obj, "clientDropped", %clientId);
}

function Game::clientKilled(%playerId, %killerId)
{
   %set = nameToID("MissionCleanup/ObjectivesSet");
   for(%i = 0; (%obj = Group::getObject(%set, %i)) != -1; %i++)
      GameBase::virtual(%obj, "clientKilled", %playerId, %killerId);
}

function Player::enterMissionArea(%this)
{
    
 %set = nameToID("MissionCleanup/ObjectivesSet");
 %this.outArea = "";
 
 %playerClient = Player::getClient(%this);
 %playerTeam = GameBase::getTeam(%this);
 %ClientName = Client::getName(%playerClient);
    
    if (%playerTeam == 0) {
        %flagTeam = 1;
    }
    else {
       %flagTeam = 0; 
    }
    
 if((%playerClient == $freeze::FlagClient[%flagTeam]) && (!$TwoMinWarning)) {
     
    $freeze::OOB[%flagTeam] = false;

 }

 for(%i = 0; (%obj = Group::getObject(%set, %i)) != -1; %i++)
  GameBase::virtual(%obj, "playerEnterMissionArea", %this);

 Client::sendMessage(Player::getClient(%this),0,"You have entered the mission area.~wLeftMissionArea.wav");
}

function Player::leaveMissionArea(%this)
{
    %playerClient = Player::getClient(%this);
    %ClientName = Client::getName(%playerClient);
    %playerTeam = GameBase::getTeam(%this);
    
    if (%playerTeam == 0) {
        %flagTeam = 1;
    }
    else {
       %flagTeam = 0; 
    }
    // if we have the flag and we've entered OOB
    if((%playerClient == $freeze::FlagClient[%flagTeam]) && (!$TwoMinWarning)) {
        
        $freeze::OOB[%flagTeam] = true;
    }

 %this.outArea=1;
 Client::sendMessage(%playerClient,1,"You have left the mission area.");
 alertPlayer(%this, 3);
}

function alertPlayer(%player, %count)
{
 if(%player.outArea == 1)
 {
  if(%count > 0)
  {
   Client::sendMessage(Player::getClient(%player),0,"~wLeftMissionArea.wav");
   schedule("alertPlayer(" @ %player @ ", " @ %count - 1 @ ");",1.5,%player);
  }
  else
  {
   %set = nameToID("MissionCleanup/ObjectivesSet");
   for(%i = 0; (%obj = Group::getObject(%set, %i)) != -1; %i++)
    GameBase::virtual(%obj, "playerLeaveMissionArea", %player);
  }
 }
}

function checkObjectives(%this)
{
   //echo("checking for objective player leave mission area...");
}

// objective init must return true
function TowerSwitch::objectiveInit(%this)
{
   return %this.scoreValue || %this.deltaTeamScore;
}

function TowerSwitch::onAdd(%this)
{
 %this.numSwitchTeams = 0;
}

function TowerSwitch::onDamage()
{
   // tower switches can't take damage
}

function TowerSwitch::getObjectiveString(%this, %forTeam)
{
   %thisTeam = GameBase::getTeam(%this);

   if($missionComplete)
   {
      if(%thisTeam == -1)
         return "<Btowers_neutral.bmp>\nNo team claimed " @ %this.objectiveName @ ".";
      else if(%thisTeam == %forTeam)
         return "<Btower_teamcontrol.bmp>\nYour team finished the mission in control of " @ %this.objectiveName @ ".";
      else {
        if(%forTeam != -1)
      return "<Btower_enemycontrol.bmp>\nThe " @ getTeamName(%thisTeam) @ " team finished the mission in control of " @ %this.objectiveName @ ".";
     else
      return "<Btower_teamcontrol.bmp>\nThe " @ getTeamName(%thisTeam) @ " team finished the mission in control of " @ %this.objectiveName @ ".";
  }
 }
   else
   {
  if(%forTeam != -1) {
       if(%this.deltaTeamScore)
       {
    if(%thisTeam == -1)
        return "<Btowers_neutral.bmp>\nClaim " @ %this.objectiveName @ " to gain " @ %this.deltaTeamScore @ " points per minute.";
       else if(%thisTeam == %forTeam)
          return "<Btower_teamcontrol.bmp>\nDefend " @ %this.objectiveName @ " to retain " @ %this.deltaTeamScore @ " points per minute.";
       else
          return "<Btower_enemycontrol.bmp>\nCapture " @ %this.objectiveName @ " from the " @ getTeamName(%thisTeam) @ " team to gain " @ %this.deltaTeamScore @ " points per minute.";
   }
       else if(%this.scoreValue)
       {
        if(%thisTeam == -1)
             return "<Btowers_neutral.bmp>\nClaim and defend " @ %this.objectiveName @ " to gain " @ %this.scoreValue @ " points.";
          else if(%thisTeam == %forTeam)
             return "<Btower_teamcontrol.bmp>\nDefend " @ %this.objectiveName @ " to retain " @ %this.scoreValue @ " points.";
          else
             return "<Btower_enemycontrol.bmp>\nCapture " @ %this.objectiveName @ " from the " @ getTeamName(%thisTeam) @ " team to gain " @ %this.deltaTeamScore @ " points.";
        }
  }
  else {
    if(%thisTeam == -1)
       return "<Btowers_neutral.bmp>\n" @ %this.objectiveName @ " has not been claimed.";
    else
       return "<Btower_teamcontrol.bmp>\nThe " @ getTeamName(%thisTeam) @ " team is in control of the " @ %this.objectiveName @ ".";
    }
   }
}

function TowerSwitch::onCollision(%this, %object)
{
   //echo("switch collision ", %object);
   if(getObjectType(%object) != "Player")
      return;

   if(Player::isDead(%object))
      return;

   %playerTeam = GameBase::getTeam(%object);
   %oldTeam = GameBase::getTeam(%this);
   if(%oldTeam == %playerTeam)
      return;

   %this.trainingObjectiveComplete = true;

   %playerClient = Player::getClient(%object);
   %touchClientName = Client::getName(%playerClient);
   %group = GetGroup(%this);
   Group::iterateRecursive(%group, GameBase::setTeam, %playerTeam);

   %dropPoints = nameToID(%group @ "/DropPoints");
   %oldDropSet = nameToID("MissionCleanup/TeamDrops" @ %oldTeam);
   %newDropSet = nameToID("MissionCleanup/TeamDrops" @ %playerTeam);

   $deltaTeamScore[%oldTeam] -= %this.deltaTeamScore;
   $deltaTeamScore[%playerTeam] += %this.deltaTeamScore;
   $teamScore[%oldTeam] -= %this.scoreValue;
   $teamScore[%playerTeam] += %this.scoreValue;

   if(%dropPoints != -1)
   {
      for(%i = 0; (%dropPoint = Group::getObject(%dropPoints, %i)) != -1; %i++)
      {
         if(%oldDropSet != -1)
            removeFromSet(%oldDropSet, %dropPoint);
         addToSet(%newDropSet, %dropPoint);
      }
   }

   if(%oldTeam == -1)
   {
      MessageAllExcept(%playerClient, 0, %touchClientName @ " claimed " @ %this.objectiveName @ " for the " @ getTeamName(%playerTeam) @ " team!");
      Client::sendMessage(%playerClient, 0, "You claimed " @ %this.objectiveName @ " for the " @ getTeamName(%playerTeam) @ " team!");
  }
   else
   {
      if(%this.objectiveLine)
      {
         MessageAllExcept(%playerClient, 0, %touchClientName @ " captured " @ %this.objectiveName @ " from the " @ getTeamName(%oldTeam) @ " team!");
         Client::sendMessage(%playerClient, 0, "You captured " @ %this.objectiveName @ " from the " @ getTeamName(%oldTeam) @ " team!");
   %this.numSwitchTeams++;
   schedule("TowerSwitch::timeLimitCheckPoints(" @ %this @ "," @ %playerClient @ "," @ %this.numSwitchTeams @ ");",60);
      }
   }
   if(%this.objectiveLine)
   {
      TeamMessages(1, %playerTeam, "Your team has taken an objective.~wCapturedTower.wav");
  TeamMessages(0, %playerTeam, "The " @ getTeamName(%playerTeam) @ " has taken an objective.");
  if(%oldTeam != -1)
       TeamMessages(1, %oldTeam, "The " @ getTeamName(%playerTeam) @ " team has taken your objective.~wLostTower.wav");
      ObjectiveMission::ObjectiveChanged(%this);
   }
   ObjectiveMission::checkScoreLimit();
}

function TowerSwitch::timeLimitCheckPoints(%this,%client,%numChange)
{
   //give player 5 points for capturing tower!
 if(%this.numSwitchTeams == %numChange) {
    %client.score+=5;
  Game::refreshClientScore(%client);
    Client::sendMessage(%client, 0, "You receive 5 points for holding your captured tower!");
 }
}

function TowerSwitch::clientKilled(%this, %playerId, %killerId)
{
   if(!%this.objectiveLine)
      return;

   %killerTeam = Client::getTeam(%killerId);
   %playerTeam = Client::getTeam(%playerId);
   %killerPos = GameBase::getPosition(%killerId);

   if(%killerId && (%playerTeam != %killerTeam))
   {
      %dist = Vector::getDistance(%killerPos, GameBase::getPosition(%this));
      //echo(%dist);
      if(%dist <= 80)
      {
         //echo("distance to objective" @ %this @ " : " @ %dist);
         if(GameBase::getTeam(%this) == Client::getTeam(%killerId) && getObjectType(%killerId) == "Player")
         {
            %killerId.score++;
            Game::refreshClientScore(%killerId);
            messageAll(0, strcat(Client::getName(%killerId), " receives a bonus for defending " @ %this.objectiveName @ "."));
         }
      }
   }
}

// objective init must return true
function Flag::objectiveInit(%this)
{
  %this.originalPosition = GameBase::getPosition(%this);
  %this.atHome = true;
  %this.pickupSequence = 0;
  %this.carrier = -1;
  %this.holdingTeam = -1;
  %this.holder = "";
  %this.changeTeamCount = 0;

  %this.enemyCaps = 0;
  %this.caps[0] = 0;
  %this.caps[1] = 0;
  %this.caps[2] = 0;
  %this.caps[3] = 0;
  %this.caps[4] = 0;
  %this.caps[5] = 0;
  %this.caps[6] = 0;
  %this.caps[7] = 0;

  %flagTeam = GameBase::getTeam(%this);
  $teamFlag[%flagTeam] = %this;
  $teamFlagStandPos[%flagTeam] = GameBase::getPosition(%this);

  return true;
}

function Flag::getObjectiveString(%this, %forTeam)
{
   %thisTeam = GameBase::getTeam(%this);

   if($missionComplete)
   {
      if(%thisTeam == -1)
      {
         if(%this.holdingTeam == %forTeam && %forTeam != -1)
            return "<Bflag_atbase.bmp>\nYour team finished the mission in control of " @ %this.objectiveName @ ".";
         else if(%this.holdingTeam == -1)
            return "<Bflag_neutral.bmp>\nNo team finished the mission in control of " @ %this.objectiveName @ ".";
         else {
    if(%forTeam != -1)
     return "<Bflag_enemycaptured.bmp>\nThe " @ getTeamName(%this.holdingTeam) @ " team finished the mission in control of " @ %this.objectiveName @ ".";
        else
     return "<Bflag_atbase.bmp>\nThe " @ getTeamName(%this.holdingTeam) @ " team finished the mission in control of " @ %this.objectiveName @ ".";
   }
  }
      else if(%forTeam != -1)
      {
         if(%thisTeam == %forTeam)
            return "<Bflag_atbase.bmp>\nYour flag was captured " @ %this.enemyCaps @ " times.";
         else
            return "<Bflag_enemycaptured.bmp>\nYour team captured the " @ getTeamName(%thisTeam) @ " flag " @ %this.caps[%forTeam] @ " times.";
      }
    else
       return "<Bflag_atbase.bmp>\nThe " @ getTeamName(%thisTeam) @ "'s flag was captured " @ %this.enemyCaps @ " times.";
   }
   else
   {
      if(%thisTeam == -1)
      {
   if(%forTeam != -1) {
          if(%this.holdingTeam == %forTeam)
             return "<Bflag_atbase.bmp>\nDefend " @ %this.objectiveName @ ".";
          else if(%this.holdingTeam != -1)
             return "<Bflag_enemycaptured.bmp>\nGrab " @ %this.objectiveName @ " from the " @ getTeamName(%this.holdingTeam) @ " team.";
          else if(%this.carrier != -1)
          {
             if(GameBase::getTeam(%this.carrier) == %forTeam)
                return "<Bflag_atbase.bmp>\nConvey " @ %this.objectiveName @ " to an empty flag stand. (carried by " @ Client::getName(Player::getClient(%this.carrier)) @ ")";
             else
                return "<Bflag_enemycaptured.bmp>\nWaylay " @ Client::getName(Player::getClient(%this.carrier)) @ " and convey " @ %this.objectiveName @ " to your base.";
          }
          else if(%this.atHome)
             return "<Bflag_neutral.bmp>\nGrab " @ %this.objectiveName @ " and convey it to an empty flag stand.";
          else
             return "<Bflag_notatbase.bmp>\nFind " @ %this.objectiveName @ " and convey it to an empty flag stand.";
       }
   else {
          if(%this.holdingTeam != -1)
             return "<Bflag_atbase.bmp>\nThe " @ getTeamName(%this.holdingTeam) @ " team has " @ %this.objectiveName @ ".";
          else if(%this.carrier != -1)
               return "<Bflag_atbase.bmp>\n" @ Client::getName(Player::getClient(%this.carrier)) @ " has " @ %this.objectiveName @ ".";
          else if(%this.atHome)
             return "<Bflag_neutral.bmp>\n" @ %this.objectiveName @ " has not been found.";
    else
             return "<Bflag_notatbase.bmp>\n" @ %this.objectiveName @ " has been dropped in the field.";
   }
  }
      else
      {
         if(%thisTeam == %forTeam)
         {
            if(%this.atHome)
               return "<Bflag_atbase.bmp>\nDefend your flag to prevent enemy captures.";
            else if(%this.carrier != -1)
               return "<Bflag_enemycaptured.bmp>\nReturn your flag to base. (carried by " @ Client::getName(Player::getClient(%this.carrier)) @ ")";
            else
               return "<Bflag_notatbase.bmp>\nReturn your flag to base. (dropped in the field)";
         }
         else
         {
    if(%forTeam != -1) {
             if(%this.atHome)
                return "<Bflag_enemycaptured.bmp>\nGrab the " @ getTeamName(%thisTeam) @ " flag and touch it to your's to score " @ %this.scoreValue @ " points.";
             else if(%this.carrier == -1)
                return "<Bflag_notatbase.bmp>\nFind the " @ getTeamName(%thisTeam) @ " flag and touch it to your's to score " @ %this.scoreValue @ " points.";
             else if(GameBase::getTeam(%this.carrier) == %forTeam)
                return "<Bflag_atbase.bmp>\nEscort friendly carrier " @ Client::getName(Player::getClient(%this.carrier)) @ " to base.";
             else
                return "<Bflag_enemycaptured.bmp>\nWaylay enemy carrier " @ Client::getName(Player::getClient(%this.carrier)) @ " and steal his flag.";
          }
    else {
             if(%this.atHome)
                return "<Bflag_atbase.bmp>\nThe " @ getTeamName(%thisTeam) @ " flag is at their base.";
             else if(%this.carrier == -1)
                return "<Bflag_notatbase.bmp>\nThe " @ getTeamName(%thisTeam) @ " flag has been dropped in the field.";
             else
                return "<Bflag_atbase.bmp>\n" @ Client::getName(Player::getClient(%this.carrier)) @ " has the " @ getTeamName(%thisTeam) @ " flag.";
    }
   }
      }
   }
}

function Flag::onDrop(%player, %type)
{

 if ($NoFlagThrow) { return; }

 %playerTeam = GameBase::getTeam(%player);
 %flag = %player.carryFlag;
 %flagTeam = GameBase::getTeam(%flag);
 %playerClient = Player::getClient(%player);
 %dropClientName = Client::getName(%playerClient);

 $Stats::FlagDropped[%flagTeam] = getIntegerTime(true) >> 5;
 $Stats::PlayerDropped[%flagTeam] = %playerClient;

 if(%flagTeam == -1)
 {
  MessageAllExcept(%playerClient, 1, %dropClientName @ " dropped " @ %flag.objectiveName @ "!");
  Client::sendMessage(%playerClient, 1, "You dropped "  @ %flag.objectiveName @ "!");
 }
 else
 {

  MessageAllExcept(%playerClient, 0, %dropClientName @ " dropped the " @ getTeamName(%flagTeam) @ " flag!");
  Client::sendMessage(%playerClient, 0, "You dropped the " @ getTeamName(%flagTeam) @ " flag!");
  TeamMessages(1, %flagTeam, "Your flag was dropped in the field.", -2, "", "The " @ getTeamName(%flagTeam) @ " flag was dropped in the field.");

  //afk monitor
  %playerClient.lastActiveTimestamp = getSimTime();

  //active code
  zadmin::ActiveMessage::All(FlagDropped, %flagTeam, %playerClient);
  Client::onFlagDrop(%flagTeam, %playerClient);
 }

 GameBase::throw(%flag, %player, 10, false);
 Item::hide(%flag, false);
 Player::setItemCount(%player, "Flag", 0);
 %flag.carrier = -1;
 %player.carryFlag = "";
 Flag::clearWaypoint(%playerClient, false);

 $FlagIsDropped[%flagTeam] = true;
 $freeze::OOB[%flagTeam] = false;
 $freeze::FlagClient[%flagTeam] = 0;
 
 Observer::CheckFlagStatus(%flagTeam);

 schedule("Flag::checkReturn(" @ %flag @ ", " @ %flag.pickupSequence @ ");", $flagReturnTime);
 %flag.dropFade = 1;
 ObjectiveMission::ObjectiveChanged(%flag);
}

function Flag::checkReturn(%flag, %sequenceNum)
{
//messageAll(0, "checking flag return...");
 if(%flag.pickupSequence == %sequenceNum && %flag.timerOn == "")
 {
  if(%flag.dropFade)
  {
   GameBase::startFadeOut(%flag);
   %flag.dropFade= "";
   %flag.fadeOut= 1;
   schedule("Flag::checkReturn(" @ %flag @ ", " @ %sequenceNum @ ");", 2.5);
  }
  else
  {

   %flagTeam = GameBase::getTeam(%flag);
   TeamMessages(0, %flagTeam, "Your flag was returned to base.~wflagreturn.wav", -2, "", "The " @ getTeamName(%flagTeam) @ " flag was returned to base.~wflagreturn.wav");
   GameBase::setPosition(%flag, %flag.originalPosition);
   Item::setVelocity(%flag, "0 0 0");

   //active code
   zadmin::ActiveMessage::All(FlagReturned, %flagTeam, 0); //server returns
   Client::onFlagReturn(%flagTeam, 0);

   $FlagIsDropped[%flagTeam] = false;
   $freeze::OOB[%flagTeam] = false;
   
   Observer::CheckFlagStatus(%flagTeam);
   
   $freeze::FlagClient[%flagTeam] = 0;
   
   %flag.atHome = true;
   GameBase::startFadeIn(%flag);
   %flag.fadeOut= "";
   ObjectiveMission::ObjectiveChanged(%flag);
  }
 }
}

function Flag::onCollision(%this, %object)
{
    if ($Server::Halftime)
        return;
    if(getObjectType(%object) != "Player")
        return;
    if(%this.carrier != -1)
        return; // spurious collision
    if(Player::isAIControlled(%object))
        return;

  //gather various variablez
  %name = Item::getItemData(%this);
  %playerTeam = GameBase::getTeam(%object);
  %flagTeam = GameBase::getTeam(%this);
  %playerClient = Player::getClient(%object);
  %touchClientName = Client::getName(%playerClient);
  %atHome = %this.atHome;
  %enemyFlagTeam = (%playerTeam + 1) % 2;


  if(%flagTeam == %playerTeam)
  {
    // player is touching his own flag...
    if(!%this.atHome)
    {

      // the flag isn't home! so return it.
      GameBase::startFadeOut(%this);
      GameBase::setPosition(%this, %this.originalPosition);
      Item::setVelocity(%this, "0 0 0");
      GameBase::startFadeIn(%this);
      %this.atHome = true;
      
      $FlagIsDropped[%flagTeam] = false;
      $freeze::OOB[%flagTeam] = false;
      
      $freeze::FlagClient[%flagTeam] = 0;
      
      Observer::CheckFlagStatus(%flagTeam);
      
      MessageAllExcept(%playerClient, 0, %touchClientName @ " returned the " @ getTeamName(%playerTeam) @ " flag!~wflagreturn.wav");
      Client::sendMessage(%playerClient, 0, "You returned the " @ getTeamName(%playerTeam) @ " flag!~wflagreturn.wav");
      teamMessages(1, %playerTeam, "Your flag was returned to base.", -2, "", "The " @ getTeamName(%playerTeam) @ " flag was returned to base.");

      %this.pickupSequence++;
      ObjectiveMission::ObjectiveChanged(%this);

      //afk monitor
      %playerClient.lastActiveTimestamp = getSimTime();
      
      // find distance of player to the enemy stand
      %playerStandRadius = Game::distanceToFlag(%playerClient, %enemyFlagTeam, false);
      
      //enemy flag at home - flag and stand will be the same location
      if(!$FlagIsDropped[%enemyFlagTeam] && !$freeze::FlagClient[%enemyFlagTeam]) {
        %playerFlagRadius = %playerStandRadius;
      }
      //enemy flag held by teammate - find distance of friendly carrier to the enemy stand
      else if(!$FlagIsDropped[%enemyFlagTeam] && $freeze::FlagClient[%enemyFlagTeam]) {
        %playerFlagRadius = Game::distanceToFlag($freeze::FlagClient[%enemyFlagTeam], %enemyFlagTeam, false);
      }
      //enemy flag is in field - find distance of enemy flag to its stand
      else {
        %playerFlagRadius = Game::distanceToFlag($teamFlag[%enemyFlagTeam], %enemyFlagTeam, false);
      }
      //player must be within distance of enemy stand as well as enemy flag
      if( (%playerFlagRadius <= $Game::ClutchReturnRadius) && (%playerStandRadius <= $Game::ClutchReturnRadius) ) 
      {
            zadmin::ActiveMessage::All(FlagClutchReturn, %playerClient, %flagTeam);
      }
      
      Client::onFlagReturn(%flagTeam, %playerClient);

      //active code
      zadmin::ActiveMessage::All(FlagReturned, %flagTeam, %playerClient);
      
      //midair return stat
      if(!Player::ObstructionsBelow(%playerClient, $Game::Midair::Height))
      {
        zadmin::ActiveMessage::All(FlagInterception, %flagTeam, %playerClient);
      }

    }
    else
    {
      // it's at home - see if we have an enemy flag!
      if(%object.carryFlag != "")
      {
        // can't cap the neutral flags, duh
        %enemyTeam = GameBase::getTeam(%object.carryFlag);
        if(%enemyTeam != -1)
        {
          MessageAllExcept(%playerClient, 0, %touchClientName @ " captured the " @ getTeamName(%enemyTeam) @ " flag!~wflagcapture.wav");
          Client::sendMessage(%playerClient, 0, "You captured the " @ getTeamName(%enemyTeam) @ " flag!~wflagcapture.wav");
          TeamMessages(1, %playerTeam, "Your team captured the flag.", %enemyTeam, "Your team's flag was captured.");

          %flag = %object.carryFlag;
          %flag.atHome = true;
          %flag.carrier = -1;
          %flag.caps[%playerTeam]++;
          %flag.enemyCaps++;
          
          $FlagIsDropped[%playerTeam] = false;
          $FlagIsDropped[%enemyTeam] = false;

          $freeze::FlagClient[%playerTeam] = 0;
          $freeze::FlagClient[%enemyTeam] = 0;
          
          $freeze::OOB[%playerTeam] = false;
          $freeze::OOB[%enemyTeam] = false;
          
          Observer::CheckFlagStatus(0);
          Observer::CheckFlagStatus(1);

          Item::hide(%flag, false);
          
          GameBase::startFadeOut(%flag);
          GameBase::setPosition(%flag, %flag.originalPosition);
          Item::setVelocity(%flag, "0 0 0");
          GameBase::startFadeIn(%flag);

          %flag.trainingObjectiveComplete = true;
          ObjectiveMission::ObjectiveChanged(%flag);

          Player::setItemCount(%object, Flag, 0);
          %object.carryFlag = "";
          Flag::clearWaypoint(%playerClient, true);

          $teamScore[%playerTeam] += %flag.scoreValue;

          //afk monitor
          %playerClient.lastActiveTimestamp = getSimTime();

          //active code
          zadmin::ActiveMessage::All(FlagCaptured, %enemyTeam, %playerClient);
          zadmin::ActiveMessage::All(TeamScore, %playerTeam, $teamScore[%playerTeam]);
          Client::onFlagCap(%enemyTeam, %playerClient);

          ObjectiveMission::checkScoreLimit();
        }
      }
    }
  }
  else
  {
    // it's an enemy's flag! woohoo!
    if(%object.carryFlag == "")
    {
      if(%object.outArea == "")
      {
        // don't pick up our flags
        if(%this.holdingTeam == %playerTeam)
          return;

      Player::setItemCount(%object, Flag, 1);
      Player::mountItem(%object, Flag, $FlagSlot, %flagTeam);
      Item::hide(%this, true);
      
      //THIS FLAG HAS BEEN PICKED UP
      $FlagIsDropped[%flagTeam] = false;
      $freeze::OOB[%flagTeam] = false;
      $freeze::FlagClient[%flagTeam] = %playerClient;
      
      Observer::CheckFlagStatus(%flagTeam);

      %this.atHome = false;
      %this.carrier = %object;
      %this.pickupSequence++;
      %object.carryFlag = %this;
      Flag::setWaypoint(%playerClient, %this);
      if(%this.fadeOut)
      {
        GameBase::startFadeIn(%this);
        %this.fadeOut= "";
      }

    if(%flagTeam != -1)
    {

      MessageAllExcept(%playerClient, 0, %touchClientName @ " took the " @ getTeamName(%flagTeam) @ " flag! ~wflag1.wav");
      Client::sendMessage(%playerClient, 0, "You took the " @ getTeamName(%flagTeam) @ " flag! ~wflag1.wav");
      TeamMessages(1, %playerTeam, "Your team has the " @ getTeamName(%flagTeam) @ " flag.", %flagTeam, "Your team's flag has been taken.");

      //afk monitor
      %playerClient.lastActiveTimestamp = getSimTime();
      
      //active code
      zadmin::ActiveMessage::All(FlagTaken, %flagTeam, %playerClient);
      
      //midair flag catch
      if(!Player::ObstructionsBelow(%playerClient, $Game::Midair::Height))
      {

        zadmin::ActiveMessage::All(FlagCatch, %flagTeam, %playerClient);
            
      }

      if (%atHome) {
        Client::onFlagGrab(%flagTeam, %playerClient);
        
        //stats - check for e-grab
        //if flag is at home - on stand
        //check radius distance to their own flag
        
        %doNotCheck = false;
        
        //your flag in field - find distance of your flag to enemy stand
        if ($FlagIsDropped[%playerTeam]) {
            %playerFlagRadius = Game::distanceToFlag($teamFlag[%playerTeam], %enemyFlagTeam, false);
        }
        //your flag on enemies back - find distance of enemy carrier to their stand
        else if ($freeze::FlagClient[%playerTeam]) {
            
            %playerFlagRadius = Game::distanceToFlag($freeze::FlagClient[%playerTeam], %enemyFlagTeam, false);

        }
        //your flag at home
        else { %doNotCheck = true; }
        
        if(%playerFlagRadius <= $Game::EgrabRadius && !%doNotCheck) {
            zadmin::ActiveMessage::All(FlagEgrab, %playerClient, %enemyFlagTeam);
        }
      }
      else {
        Client::onFlagPickup(%flagTeam, %playerClient);
      }
    }

    %this.trainingObjectiveComplete = true;
    ObjectiveMission::ObjectiveChanged(%this);
  }
  else
    Client::sendMessage(%playerClient, 1, "Flag not in mission area.");
  }
 }
}

function Flag::clearWaypoint(%client, %success)
{
   if(%success)
      setCommandStatus(%client, 0, "Objective completed.~wobjcomp");
   else
      setCommandStatus(%client, 0, "Objective failed.");
}

function Flag::setWaypoint(%client, %flag)
{
   if(!%client.autoWaypoint)
      return;
   %flagTeam = GameBase::getTeam(%flag);
   %team = Client::getTeam(%client);

  %pos = ($teamFlag[%team]).originalPosition;
  %posX = getWord(%pos,0);
  %posY = getWord(%pos,1);
      issueCommand(%client, %client, 0,"Take the " @ getTeamName(%flagTeam) @ " flag to our flag.~wcapobj", %posX, %posY);
  return;
}

function Flag::clientKilled(%this, %playerId, %killerId)
{
   //flag stand function?
}

function Flag::clientDropped(%this, %clientId)
{
   //echo(%this @ " " @ %clientId);
   %type = Player::getMountedItem(%clientId, $FlagSlot);
   if(%type != -1)
      Player::dropItem(%clientId, %type);
}

function Flag::playerLeaveMissionArea(%this, %playerId)
{
   // if a guy leaves the area, warp the flag back to its base
 if(%this.carrier == %playerId)
 {
  GameBase::startFadeOut(%this);
  Player::setItemCount(%playerId, "Flag", 0);
  %playerClient = Player::getClient(%playerId);
  %clientName = Client::getName(%playerClient);
  %flagTeam = GameBase::getTeam(%this);
  //Stats::FlagTime::StopTimer(%flagTeam, %playerClient);
  
   if(%flagTeam != -1)
   {
    %team = %flagTeam;
    GameBase::setPosition(%this, %this.originalPosition);
    Item::setVelocity(%this, "0 0 0");
   }
   else
   {
    %team = GameBase::getTeam(%this.flagStand);
    GameBase::setPosition(%this, GameBase::getPosition(%this.flagStand));
    Item::setVelocity(%this, "0 0 0");
   }

   MessageAllExcept(%playerClient, 0, %clientName @ " left the mission area while carrying the " @ getTeamName(%team) @ " flag!");
   Client::sendMessage(%playerClient, 0, "You left the mission area while carrying the " @ getTeamName(%team) @ " flag!");
   TeamMessages(1, %team, "Your flag was returned to base.~wflagreturn.wav", -2, "", "The " @ getTeamName(%team) @ " flag was returned to base.~wflagreturn.wav");

   %holdTeam = GameBase::getTeam(%this.flagStand);
   $teamScore[%holdTeam] += %this.scoreValue;
   $deltaTeamScore[%holdTeam] += %this.deltaTeamScore;
   %this.holder = %this.flagStand;
   %this.flagStand.flag = %this;
   %this.holdingTeam = %holdTeam;

   //fix
   %this.atHome = true;
   
   $FlagIsDropped[%flagTeam] = false;
   $freeze::FlagClient[%flagTeam] = 0;
   $freeze::OOB[%flagTeam] = false;
   
   Observer::CheckFlagStatus(%flagTeam);

   //active code
   zadmin::ActiveMessage::All(FlagReturned, %flagTeam, 0); //server returns
   Client::onFlagReturn(%flagTeam, 0);
   
  GameBase::startFadeIn(%this);
  %this.carrier = -1;
  Item::hide(%this, false);

  %playerId.carryFlag = "";
  Flag::clearWaypoint(%playerClient, false);
  ObjectiveMission::ObjectiveChanged(%this);
  ObjectiveMission::checkScoreLimit();
 }
}

function Flag::ResetFlag(%flag)
{
  // Physically move flag
  GameBase::setPosition(%flag, %flag.originalPosition);
  Item::setVelocity(%flag, "0 0 0");

  // Reset flag
  %flag.flagStand = "";
  %flagTeam = GameBase::getTeam(%flag);
  Flag::objectiveInit(%flag);

  // Notify players
  zadmin::ActiveMessage::All(FlagReturned, %flagTeam, 0); //server returns
}
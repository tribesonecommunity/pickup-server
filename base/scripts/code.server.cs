// putting a global variable in the argument list means:
// if an argument is passed for that parameter it gets
// assigned to the global scope, not the scope of the function

$Console::LastLineTimeout = 0;
$Console::updateMetrics = false;

// redefine end frame script
function Game::EndFrame()
{
}

function selectNewMaster()
{
   translateMasters();
}

function checkMasterTranslation()
{
  for(%i = 0; %i < $Server::numMasters; %i++)
  {
    %mstr = DNet::getResolvedMaster(%i);
    if(%mstr != "")
      $Server::XLMasterN[%i] = %mstr;
    $inet::master[%i+1] = $Server::XLMasterN[%i];
  }
}

function translateMasters()
{
  for(%i = 0; (%word = getWord($Server::MasterAddressN[$Server::CurrentMaster], %i)) != -1; %i++)
    %mlist[%i] = %word;
  $Server::numMasters = DNet::resolveMasters(%mlist0, %mlist1, %mlist2, %mlist3, %mlist4, %mlist5, %mlist6, %mlist7, %mlist8, %mlist9);
}

function createTrainingServer()
{
  $SinglePlayer = true;
  createServer($pref::lastTrainingMission, false);
}

function remoteSetCLInfo(%clientId, %skin, %name, %email, %tribe, %url, %info, %autowp, %enterInv, %msgMask)
{
  $Client::info[%clientId, 0] = %skin;
  $Client::info[%clientId, 1] = %name;
  $Client::info[%clientId, 2] = %email;
  $Client::info[%clientId, 3] = %tribe;
  $Client::info[%clientId, 4] = %url;
  $Client::info[%clientId, 5] = %info;
  if(%autowp)
    %clientId.autoWaypoint = true;
  if(%enterInv)
    %clientId.noEnterInventory = true;
  if(%msgMask != "")
    %clientId.messageFilter = %msgMask;
}

function Server::storeData()
{
  $ServerDataFile = "serverTempData.cs";

  //export("Server::*", "temp\\" @ $ServerDataFile, False);
  //export("pref::lastMission", "temp\\" @ $ServerDataFile, true);
  EvalSearchPath();
}

function Server::refreshData()
{
   exec($ServerDataFile);  // reload prefs.
   checkMasterTranslation();
   Server::nextMission(false);
}

function Server::onClientConnect(%clientId)
{
  banlist::add(client::getTransportAddress(%clientId), 5);

  if(string::findSubStr(client::getName(%clientId), ".bmp>") != "-1" || client::getName(%clientId) == "" || string::findSubStr(client::getName(%clientId), "<R") != "-1" || string::findSubStr(client::getName(%clientId), "<L") != "-1" || string::findSubStr(client::getName(%clientId), "<S") != "-1")
  {
  	kick(%clientId, "Get a different name.");
  	banlist::add(client::getTransportAddress(%clientId), 60);
  }
	//  Overflow stuff
	if ( $zadmin::pref::pickup::enabled || !$Server::TourneyMode )
	{
	    if (getNumClients() > $Server::MaxPlayers)
	    {
			inGameAlert(Client::getName(%clientId) @ " just OVERFLOWED ");

			%clientId.registeredName = Client::getName(%clientId);
			if ($zadmin::pref::log::Overflows)
				logEntry(%clientId, "logged in with OVERFLOW.", "", "O");

			if ($zadmin::pref::overflow::forceEnterSAD)
				schedule("overflowAdminCheck(" @ %clientId @ ");", 15);
		}

		if (PasswordCheck())
			OverflowCycle(getNumClients());
	}

	//put the last pw back on
	if (getNumClients() == 1)
		$Server::Password = $Server::CurrentPassword;

	echo("CONNECT: " @ %clientId @ " \"" @
		escapeString(Client::getName(%clientId)) @
		"\" " @ Client::getTransportAddress(%clientId));


	%clientId.noghost = true;
	%clientId.messageFilter = -1; // all messages

	remoteEval(%clientId, SVInfo, version(), $Server::Hostname, $modList, $Server::Info, $ItemFavoritesKey);
	remoteEval(%clientId, MODInfo, $MODInfo);
	remoteEval(%clientId, FileURL, $Server::FileURL);

	// clear out any client info:
	for(%i = 0; %i < 10; %i++)
		$Client::info[%clientId, %i] = "";

	Game::onPlayerConnected(%clientId);
	//IPLog::createEntry(%clientId);

	%ip = Client::getTransportAddress(%clientId);
	%name = Client::getName(%clientId);

	//client drop/rejoin recognition
	Client::Recall(%clientId);

	// just to make sure he gets fully logged, we'll check here
	if ( String::findSubStr( %name, ".bmp>" ) != -1 ) {
		Banlist::Add( %ip, 60 );
		Schedule( "Net::Kick("@%clientId@", 'Hey crap for brains: DONT USE BMP EXPLOITS' );", 10 );
		return;
	}

	for (%i=0; %i<$zadmin::globalspam::names; %i++)
		if ($zadmin::globalspam::name[%i] == %name)
			%clientId.globalMute = true;

	for (%i=0; %i<$zadmin::globalspam::ips; %i++)
		if ( String::FindSubStr(%ip, $zadmin::globalspam::ip[%i]) != -1 )
			%clientId.globalMute = true;

	for (%i=0; %i<$zadmin::megaspam::names; %i++)
		if ($zadmin::megaspam::name[%i] == %name)
			%clientId.megaMute = true;

	for (%i=0; %i<$zadmin::megaspam::ips; %i++)
		if ( String::FindSubStr(%ip, $zadmin::megaspam::ip[%i]) != -1 )
			%clientId.megaMute = true;

	$ServerIsEmpty = false;
	ObjectiveMission::refreshTeamScores();
	Game::checkTimeLimit();
}

function Server::onClientDisconnect(%clientId)
{
	//client drop/rejoin recognition
	Client::Remember(%clientId);

	//  Overflow stuff
	if (( $zadmin::pref::pickup::enabled || !$Server::TourneyMode ) && PasswordCheck())
		OverflowCycle(getNumClients() - 1);

	// Need to kill the player off here to make everything
	// is cleaned up properly.
	%player = Client::getOwnedObject(%clientId);
	if(%player != -1 && getObjectType(%player) == "Player" && !Player::isDead(%player))
	{
		playNextAnim(%player);
		Player::kill(%player);
	}

	Client::setControlObject(%clientId, -1);
	Client::leaveGame(%clientId);
	Game::CheckTourneyMatchStart();
	
	%tempNumClients = getNumClients();
	if(%tempNumClients == 1) { // this is the last client.
		Server::refreshData();
	}
	else if (%tempNumClients == 0) {
		// IF getNumclients is zero here, schedule a systematic 30 second time check until someone joins
		$ServerIsEmpty = true;
		Game::checkTimeLimit();
	}
	else {}

	ObjectiveMission::refreshTeamScores();
}



function createServer(%mission, %dedicated) {
	// hardcode packet rate
	$pref::PacketRate = "30";
	$pref::PacketSize = "450";

  $loadingMission = false;
  if(%mission == "")
    %mission = $pref::lastMission;

  if(%mission == "")
  {
    echo("Error: no mission provided.");
    return "False";
  }

  if(!$SinglePlayer)
    $pref::lastMission = %mission;

  newObject(serverDelegate, FearCSDelegate, true, "IP", $Server::Port, "IPX", $Server::Port, "LOOPBACK", $Server::Port);


  exec("code.mapproperties.cs");
  exec("code.morestring.cs");
  exec("code.active.cs");
  exec("code.projectile.cs");
  exec("code.armor.cs");
  exec("code.player.cs");
  exec("code.trigger.cs");
  exec("code.item.cs");
  exec("code.items.cs");
  exec("code.pack.cs");
  exec("code.staticshape.cs");
  exec("code.timestamp.cs");
  exec("code.afk.cs");
  exec("code.telnet.cs");
  exec("code.scoring.cs");
  exec("code.client.cs");
  exec("code.admin.cs");
  exec("code.datablocks.cs");

  Server::storeData();

  // NOTE!! You must have declared all data blocks BEFORE you call
  // preloadServerDataBlocks.

  preloadServerDataBlocks();

  Server::loadMission( ($missionName = %mission), true );

	// Overflow stuff
	if ( !$Server::TourneyMode )
	  $Server::Password = $zadmin::pref::overflow::defaultpw;
	$Server::CurrentPassword = $Server::Password;

	return "True";
}

function Server::nextMission(%replay) {
	
	// server trying to end, but if its 1st half and in balance mode, switch to half time
	if ($Server::Half == 1 && $Server::BalancedMode) {
			Game::HalfTimeNow();
            return;
    }

	if(%replay) {
		%nextMission = $missionName;
    }
    else {
        %nextMission = $nextMission[$missionName];
    }
		
    echo("Changing to mission ", %nextMission, ".");
	// give the clients enough time to load up the victory screen
    Server::loadMission(%nextMission);
}

function remoteCycleMission(%clientId)
{
  if(%clientId.isAdmin)
  {
    messageAll(0, Client::getName(%playerId) @ " cycled the mission.");
    Server::nextMission();
  }
}

function remoteDataFinished(%clientId)
{
  if(%clientId.dataFinished)
    return;
  %clientId.dataFinished = true;
  Client::setDataFinished(%clientId);
  %clientId.svNoGhost = ""; // clear the data flag
  if($ghosting)
  {
    %clientId.ghostDoneFlag = true; // allow a CGA done from this dude
    startGhosting(%clientId);  // let the ghosting begin!
  }
}

function remoteCGADone(%playerId)
{
	if(!%playerId.ghostDoneFlag || !$ghosting)
		return;

	%playerId.ghostDoneFlag = "";

	if(onPermaBanList(%playerId) && !BanExclusions::isMember(%playerId))
	{
		schedule("kickBanned(" @ %playerId @ ");", 2, %playerId);
		return;
	}

	Game::initialMissionDrop(%playerid);

	if ($cdTrack != "")
	  remoteEval (%playerId, setMusic, $cdTrack, $cdPlayMode);

	remoteEval(%playerId, MInfo, $missionName);
}


function Server::loadMission(%missionName, %immed)
{

	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
	{
		Client::setGuiMode(%cl, $GuiModeVictory);

		%cl.guiLock = true;
		%cl.nospawn = true;
		remoteEval(%cl, missionChangeNotify, %missionName);
	}

	if($loadingMission)
    return;

	%missionFile = "missions\\" $+ %missionName $+ ".mis";
	if(File::FindFirst(%missionFile) == "")
	{
		%missionName = $firstMission;
		%missionFile = "missions\\" $+ %missionName $+ ".mis";

		if(File::FindFirst(%missionFile) == "")
		{
		  echo("invalid nextMission and firstMission...");
		  echo("aborting mission load.");
		  return;
		}
	}

	echo("Notifying players of mission change: ", getNumClients(), " in game");
	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
	{
		Client::setGuiMode(%cl, $GuiModeVictory);

		%cl.guiLock = true;
		%cl.nospawn = true;
		remoteEval(%cl, missionChangeNotify, %missionName);
	}

	$loadingMission = true;
	$missionName = %missionName;
	$missionFile = %missionFile;
	$prevNumTeams = getNumTeams();

    deleteObject("MissionGroup");
    deleteObject("MissionCleanup");
    deleteObject("ConsoleScheduler");
	
	resetPlayerManager();
	resetGhostManagers();

	$ghosting = false;

	resetSimTime(); // deal with time imprecision

	newObject(ConsoleScheduler, SimConsoleScheduler);

	if(!%immed)
		schedule("Server::finishMissionLoad();", 18);
	else
		Server::finishMissionLoad();
}

function Server::finishMissionLoad()
{
	
	$FFATourney = true;
	$TestMissionType = "";

	// instant off of the manager
	setInstantGroup(0);
	newObject(MissionCleanup, SimGroup);

	exec($missionFile);

	%scoredelta = $teamscore[0] - $teamscore[1];
	if (%scoredelta < 0) %scoredelta=-%scoredelta;

	Mission::init();
	Mission::reinitData();

  //if ((getNumTeams() == 2) && !$server::tourneyMode && (%scoredelta > 3))
  if ((getNumTeams() == 2) && !$server::tourneyMode)
	{
		//randomize the teams
		for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
			GameBase::setTeam(%cl, -1);
		}
	}
	else if ($prevNumTeams != getNumTeams())
	{
		// loop thru clients and setTeam to -1;
		messageAll(0, "New teamcount - resetting teams.");

		for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
			GameBase::setTeam(%cl, -1);
	}
	$ghosting = true;

	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
	{
		if(!%cl.svNoGhost)
		{
			%cl.ghostDoneFlag = true;
			startGhosting(%cl);
		}
	}
	
	
	// For balanced mode
	Game::ResetHalf();
	
	if (!$Server::manualTourney) {
		Server::FFAReset();
	}
	else {
		Server::BalancedModeTime(true);
	}

	$Server::manualTourney = false;
	
	//VARIOUS RESET VARIABLES KTHX
	
	$matchStarted = false;
	$CountdownStarted = false;
	$SuicideTimeChecker = 0;
	$TwoMinWarning = false;
	$curTimeAdjust = false;
	
	$FFATourney = true;
	$Game::LT::Overtime = false;
	$Server::disableBalanced = false;
	
	$NoFlagThrow = false;
	$freezedata::actice = 0;
	$timeCheckSwitch = false;
	
	//last variable to get set
	$loadingMission = false;
	
	$ServerIsEmpty = false;
	%tempNumClients = getNumClients();
	if (%tempNumClients == 0) {
		$ServerIsEmpty = true;
	}
	
	if($SinglePlayer)
		Game::startMatch();
	else if($Server::warmupTime && !$Server::TourneyMode)
		Server::Countdown($Server::warmupTime);
	else if(!$Server::TourneyMode)
		Game::startMatch();

	$teamplay = (getNumTeams() != 1);
	purgeResources(true);

	return "True";
}

function Server::BalancedModeTime(%toggle)
{
	//cannot adjust balanced mode once 2nd half has started
	if ($Server::Half == 2)
		return;
	
	if (%toggle) {
		$Server::BalancedMode = $BalanceMode::Option;
		$Server::timeLimit = 15;
		messageAll(1,"Balance Mode ON!~wmine_act.wav");
		messageAll(1,"Match time limit has been adjusted to 15 minutes!");
	}
	else {
		//original time limit
		$Server::BalancedMode = 0;
		$Server::timeLimit = 30;
		messageAll(1,"Balance Mode OFF!~wmine_act.wav");
		messageAll(1,"Match time limit has been adjusted to 30 minutes!");
	}
	Game::UpdateTimeOnly();
}

function Server::FFAReset()
{
    messageAll(0, "Server switched to Free-For-All Mode.");

    $Server::TourneyMode = false;
	$Server::Half = 0;
    centerprintall(); // clear the messages
	  
	Server::BalancedModeTime(false);
}

function Server::GameTimeBalance()
{
	
	if ($Server::BalancedMode == 1) {
		$Server::timeLimit = $Server::FirstHalfTime;
	
		schedule('messageAll(1,"Balance Mode ON!~wmine_act.wav");', 1);
		schedule('messageAll(1,"Second half time limit has been adjusted to " @ $halftimeMins @ " minutes " @ $halftimeSecs @ " seconds!");', 1);
		
	}
	else if ($Server::BalancedMode == 2) {
		$Server::timeLimit = 15;
	
		schedule('messageAll(1,"Balance Mode ON!~wmine_act.wav");', 1);
		schedule('messageAll(1,"Second half time limit has been adjusted to 15 minutes!");', 1);
	}
	else { }
	Game::UpdateTimeOnly();	
}

function Server::CheckMatchStarted()
{
  // if the match hasn't started yet, just reset the map
  // timing issue.
  if(!$matchStarted)
  {
    Server::nextMission(true);
  }
}

function Server::Countdown(%time)
{
	$countdownStarted = true;
	$FFATourney = false;
	
	if (($Server::BalancedMode == 1 || $Server::BalancedMode == 2) && $Server::Half == 2) {
		
		schedule("Game::startHalf();", %time);
	}
	
	else {
		
		schedule("Game::startMatch();", %time);
	}
	

  ObjectiveMission::refreshTeamScores();
  zadmin::ActiveMessage::All( eventCountdownStarted );
  Game::notifyMatchStart(%time);
  
  if(%time > 30)
    schedule("Game::notifyMatchStart(30);", %time - 30);
  if(%time > 15)
    schedule("Game::notifyMatchStart(15);", %time - 15);
  if(%time > 10)
    schedule("Game::notifyMatchStart(10);", %time - 10);
  if(%time > 5)
    schedule("Game::notifyMatchStart(5);", %time - 5);
}

function Client::setInventoryText(%clientId, %txt)
{
  remoteEval(%clientId, "ITXT", %txt);
}

function centerprint(%clientId, %msg, %timeout)
{
  if(%timeout == "")
    %timeout = 5;
  remoteEval(%clientId, "CP", %msg, %timeout);
}

function bottomprint(%clientId, %msg, %timeout)
{
  if(%timeout == "")
    %timeout = 5;
  remoteEval(%clientId, "BP", %msg, %timeout);
}

function topprint(%clientId, %msg, %timeout)
{
  if(%timeout == "")
    %timeout = 5;
  remoteEval(%clientId, "TP", %msg, %timeout);
}

function centerprintall(%msg, %timeout)
{
  if(%timeout == "")
    %timeout = 5;
  for(%clientId = Client::getFirst(); %clientId != -1; %clientId = Client::getNext(%clientId))
    remoteEval(%clientId, "CP", %msg, %timeout);
}

function bottomprintall(%msg, %timeout)
{
  if(%timeout == "")
    %timeout = 5;
  for(%clientId = Client::getFirst(); %clientId != -1; %clientId = Client::getNext(%clientId))
    remoteEval(%clientId, "BP", %msg, %timeout);
}

function topprintall(%msg, %timeout)
{
   if(%timeout == "")
      %timeout = 5;
   for(%clientId = Client::getFirst(); %clientId != -1; %clientId = Client::getNext(%clientId))
      remoteEval(%clientId, "TP", %msg, %timeout);
}

function overflowAdminKick(%client)
{
	if (Client::GetName(%client) != $Overflow::NameCheck[%client])
		return;

	if (%client.adminLevel == 0)
	{
		%client.registeredName = Client::getName(%client);
		logEntry(%client, "failed to enter SAD in a timely manner", "", "?");
		Net::kick(%client, "You just got busted using the overflow password. Kiss Tribes goodbye..");
	}
}

function overflowAdminCheck(%client)
{
	if (!$zadmin::pref::overflow::forceEnterSAD)
		return;

	if (%client.adminLevel == 0)
	{
		Client::sendMessage(%client, 1, "You have " @ $zadmin::pref::overflow::EnterSADTime @ " seconds to enter your SAD before you are kicked for using overflow.~wLeftMissionArea.wav");
		$Overflow::NameCheck[%client] = Client::GetName(%client);
		schedule("overflowAdminKick(" @ %client @ ");", $zadmin::pref::overflow::EnterSADTime);
	}
}

function PasswordCheck()
{
	if (($Server::Password == $zadmin::pref::overflow::defaultpw) ||
		($Server::Password == $zadmin::pref::overflow::secondpw) ||
		($Server::Password == $zadmin::pref::overflow::overflowpw) ||
		($zadmin::pref::pickup::enabled))
		return 1;
	else
		return 0;
}

function OverflowCycle(%count)
{
	if (%count >= 0 && %count < $Server::MaxPlayers)
	{
		if (($zadmin::pref::overflow::secondlevel != "") && (%count >= $zadmin::pref::overflow::secondlevel))
		{
			if ( %count >= $zadmin::pref::overflow::secondlevel )
			{
				echo("Second Level PW");
				if ($zadmin::pref::pickup::enabled)
					$Server::Password = "pickup";
				else
					$Server::Password = $zadmin::pref::overflow::secondpw;
				return 1;
			}
		}
		else
		{
			echo("First Level PW");
			if ($zadmin::pref::pickup::enabled)
				$Server::Password = "pickup";
			else
				$Server::Password = $zadmin::pref::overflow::defaultpw;
			return 1;
		}
	}
	else if ( %count >= $Server::MaxPlayers && %count < ( $Server::MaxPlayers + $zadmin::pref::overflow::maxspots ) )
	{
		if ( $zadmin::pref::overflow::overflowpw != "" )
		{
			echo("Overflow Level PW");
			$Server::Password = $zadmin::pref::overflow::overflowpw;
			$Server::MaxPlayers = %count + 1;
			return 1;
		}
		else
		{
			echo("**** Overflow password CANNOT be blank. Overflow aborted! ****");
			return 0;
		}
	}
	else
	{
		echo("**** Player count out of accepted range!!! ****");
		return 0;
	}
}

function onExit()
{
}
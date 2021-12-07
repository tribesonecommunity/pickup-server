exec("code.prefs.cs");
zAdmin::InitPrefs();

exec("zadmin.missionlist.cs");
exec("code.log.cs");
exec("code.menu.cs");
exec("code.config.cs");

function zAdminInit()
{
	$maxMenuSize = 7;  //0-8 lines for options

    %serverName = "";

	//remove spaces and illegal characters
    for (%i = 0; %i < getLength($Server::HostName); %i++)
	{
	     %char = String::getSubStr($Server::hostName, %i, 1);
		 %result = String::iCompare(%char, "z");
		 if((%result >= -42 && %result <= -33) || (%result >= -25 && %result <= 0))
		     %serverName = %serverName @ %char;
    }

	%suffix = zadmin::getFileTimeStamp();

    $zAdminLogFile = "zadmin." @ %serverName @ %suffix @ ".log.cs";
	$zAdminBanLogFile = "zadmin.banlog.cs";
	$zAdminBanExclusionsFile = "zadmin.banexclusions.cs";

	$curVoteTopic = "";
	$curVoteAction = "";
	$curVoteOption = "";
	$curVoteCount = 0;
}

function awardAdminship(%client)
{
    %client.canKick 			= (%client.adminLevel >= $minAccessRequired::kick);
	%client.canBan  			= (%client.adminLevel >= $minAccessRequired::ban);
	%client.canChangeMission 	= (%client.adminLevel >= $minAccessRequired::changeMission);
	%client.canSetPassword 		= (%client.adminLevel >= $minAccessRequired::setPassword);
	%client.canChangeTimeLimit 	= (%client.adminLevel >= $minAccessRequired::changeTimeLimit);
	%client.cansetTeamInfo		= (%client.adminLevel >= $minAccessRequired::setTeamInfo);
	%client.canChangeGameMode	= (%client.adminLevel >= $minAccessRequired::changeGameMode);
	%client.canChangePlyrTeam	= (%client.adminLevel >= $minAccessRequired::changePlyrTeam);
	%client.canForceMatchStart  = (%client.adminLevel >= $minAccessRequired::forceMatchStart);
	%client.canSwitchTeamDamage = (%client.adminLevel >= $minAccessRequired::switchTeamDamage);
	%client.canMakeAdmin		= (%client.adminLevel >= $minAccessRequired::makeAdmin);
	%client.canMakeGadmin		= (%client.adminLevel >= $minAccessRequired::makeGadmin);
	%client.canMakeSadmin		= (%client.adminLevel >= $minAccessRequired::makeSadmin);
	%client.canResetServer		= (%client.adminLevel >= $minAccessRequired::resetServer);
	%client.canSeePlayerSpecs   = (%client.adminLevel >= $minAccessRequired::seePlayerSpecs);
	%client.canSendWarning		= (%client.adminLevel >= $minAccessRequired::sendWarning);
	%client.canAnnounceTakeover = (%client.adminLevel >= $minAccessRequired::announceTakeover);
	%client.canStripAdmin		= (%client.adminLevel >= $minAccessRequired::stripAdmin);
	%client.canReceiveAlerts	= (%client.adminLevel >= $minAccessRequired::receiveAlerts);
	%client.canPermanentBan		= (%client.adminLevel >= $minAccessRequired::permanentBan);
	%client.canCancelVote		= (%client.adminLevel >= $minAccessRequired::cancelVote);
	%client.canSendPrivateMsgs	= (%client.adminLevel >= $minAccessRequired::sendPrivateMsgs);
	%client.canSeePlayerlist    = (%client.adminLevel >= $minAccessRequired::seePlayerList);
	%client.canAntiRape         = (%client.adminLevel >= $minAccessRequired::antiRape);
	%client.canAntiRepair       = (%client.adminLevel >= $minAccessRequired::antiRepair);
	%client.canPickup           = (%client.adminLevel >= $minAccessRequired::pickupMode);
	%client.isAdmin 			= %client.adminLevel > 0;
}

function remoteSetPassword(%client, %password)
{
   if(%client.canSetPassword)
   {
      $Server::Password = %password;

	  if($zadmin::pref::log::PasswordChanges) logEntry(%client, "changed the password to" @ %password, "");
   }
}

function remoteSetTimeLimit(%client, %time)
{
   %time = floor(%time);
   if(%time == $Server::timeLimit || (%time != 0 && %time < 1))
      return;
   if(%client.canChangeTimeLimit)
   {
      if($zadmin::pref::log::TimeChanges) logEntry(%client, "changed time limit to " @ %time, "");
      $Server::timeLimit = %time;
      if(%time)
         messageAll(0, Client::getName(%client) @ " changed the time limit to " @ %time @ " minute(s).");
      else
         messageAll(0, Client::getName(%client) @ " disabled the time limit.");

   }
}

function remoteSetTeamInfo(%client, %team, %teamName, %skinBase)
{
   if(%team >= 0 && %team < 8 && %client.canSetTeamInfo)
   {
      //if($logNameSkinChanges) logEntry(%client, "set team " @ %team @ " name to " @ %teamName @ " and skin to " @ %skinBase, "");
      $Server::teamName[%team] = %teamName;
      $Server::teamSkin[%team] = %skinBase;
      messageAll(0, "Team " @ %team @ " is now \"" @ %teamName @ "\" with skin: "
         @ %skinBase @ " courtesy of " @ Client::getName(%client) @ ".  Changes will take effect next mission.");
   }
}


function remoteSelectClient(%clientId, %selId)
{
   if(%clientId.selClient != %selId)
   {
      %clientId.selClient = %selId;
	  Game::menuRequest(%clientId);

	  if (%selId.registeredName == "")
	     %selId.registeredName = "Unknown";
	  if(!%selId.adminLevel)
	     %selId.adminLevel = 0;

      if(%clientId.canSeePlayerSpecs)
      {
	      if(%clientId.canSendPrivateMsgs)
			 remoteEval(%clientId, "setInfoLine", 1, "**PVT MESSAGING ACTIVE**");
		  else
	         remoteEval(%clientId, "setInfoLine", 1, "Player Info for " @ Client::getName(%selId) @ ":");

	      remoteEval(%clientId, "setInfoLine", 2, "Admin Status: " @ $accessLevel::[%selId.adminLevel]);
	      remoteEval(%clientId, "setInfoLine", 3, "Name: " @ %selId.registeredName);
	      remoteEval(%clientId, "setInfoLine", 4, "IP: " @ Client::getTransportAddress(%selId));

	      if(%clientId.canSendPrivateMsgs  && %clientId == %selId)
		  {
			 remoteEval(%clientId, "setInfoLine", 5, "");
			 remoteEval(%clientId, "setInfoLine", 6, "CHAT now Broadcasts message.");
		  }
		  else if(%clientId.canSendPrivateMsgs  && %clientId != %selId)
		  {
			 remoteEval(%clientId, "setInfoLine", 5, "");
			 remoteEval(%clientId, "setInfoLine", 6, "CHAT now /pm's " @ Client::getName(%selId));
		  }

      }
      else
      {
	      remoteEval(%clientId, "setInfoLine", 1, "Player Info for " @ Client::getName(%selId) @ ":");
	      remoteEval(%clientId, "setInfoLine", 2, "Real Name: " @ $Client::info[%selId, 1]);
	      remoteEval(%clientId, "setInfoLine", 3, "Email Addr: " @ $Client::info[%selId, 2]);
	      remoteEval(%clientId, "setInfoLine", 4, "Tribe: " @ $Client::info[%selId, 3]);
	      remoteEval(%clientId, "setInfoLine", 5, "URL: " @ $Client::info[%selId, 4]);
	      remoteEval(%clientId, "setInfoLine", 6, "Other: " @ $Client::info[%selId, 5]);
	  }
   }

   %clientId.tries++;
   if(%clientId.tries > 10)
   {
	if(%clientId.gone)
	{
		return;
	}
	%name = client::getName(%clientId);
	Log::Exploit(%clientId, "Server-Crash", "remoteSelectClient");
	banlist::add(client::getTransportAddress(%clientId), 999);
	kick(%clientId, "You Were Kicked For Spamming remoteSelectClient");
	%clientId.gone = true;
	return;
   }
    schedule(%clientId@".tries = 0;", 0.5);
}


function remoteVoteYes(%clientId)
{
   %clientId.vote = "yes";
   centerprint(%clientId, "", 0);
}

function remoteVoteNo(%clientId)
{
   %clientId.vote = "no";
   centerprint(%clientId, "", 0);
}

function a(){}

function buildNewMenu(%displayName, %menuHandle, %cl)
{
   Client::buildMenu(%cl, %displayName, %menuHandle, true);
   %cl.menuLine = 0;
}

function addLine(%item, %itemResult, %condition, %cl)
{
    if(%condition)
	   Client::addMenuItem(%cl, %cl.menuLine++ @ %item, %itemResult);
}

function Game::menuRequest(%cl)
{
   %cl.tries++;
if(%cl.tries > 10)
{
	if(%cl.gone)
	{
		return;
	}
	%name = Client::GetName(%cl);
	Log::Exploit(%cl, "Server-Crash", "remoteScoresOn");
	banlist::add(client::getTransportAddress(%cl), 999);
	kick(%cl, "You Were Kicked For Spamming remoteScoresOn");
	%cl.gone = true;
	return;
}
	schedule(%cl@".tries = 0;", 0.5);
   if(%cl.selClient && %cl.selClient != %cl)
      displayMenuNonSelfSelMenu(%cl);

   else if(%cl.selClient == %cl)
      displayMenuSelfSelMenu(%cl);

   else if($curVoteTopic != "" && (%cl.vote == "" || %cl.canCancelVote))
      displayMenuVotePendingMenu(%cl);

   else if(%cl.adminLevel)
   	  displayMenuAdminMenu(%cl);

   else
      displayMenuVoteMenu(%cl);
}

function a(){}

function displayMenuAdminMenu(%cl)
{
	ObjectiveMission::refreshTeamScores();
	
	%rec = %cl.selClient;
    %recName = Client::getName(%rec);
	%tModeWaiting = ($Server::TourneyMode && !$CountdownStarted);

    buildNewMenu("Main Options", "adminmenu", %cl);

	addLine("Change Teams/Observe", "changeteams", (!$freezedata::actice && !$loadingMission && %cl.adminLevel > 0), %cl);
    addLine("Change mission", "changeMission", (!$freezedata::actice && %cl.canChangeMission && !$loadingMission), %cl);
	
	if ($BalanceMode::Option == 1) {
		addLine("Set Time Limit", "ctimelimit", (%cl.canChangeTimeLimit && !$loadingMission && $Server::Half != 2), %cl);
		addLine("Time Limit: [" @ $halftimeMins @ " mins " @ $halftimeSecs @ " secs]", "", (%cl.canChangeTimeLimit && !$loadingMission && $Server::Half == 2), %cl);
	}
	else {
		addLine("Set Time Limit", "ctimelimit", (%cl.canChangeTimeLimit && !$loadingMission), %cl);
		//addLine("Time Limit: [" @ $halftimeMins @ " mins " @ $halftimeSecs @ " secs]", "", (%cl.canChangeTimeLimit && !$loadingMission && $Server::Half == 2), %cl);
	}
	
	addLine("Admin options...", "serverToggles", !$freezedata::actice, %cl);
	addLine("Vote options...", "voteOptions", !$freezedata::actice, %cl);
	
	//normally you would do for loop to get num of teams but we dont care here
	
	%curTimePause = floor(($Server::timeLimit * 60) + $missionStartTime - getSimTime());
	
	addLine("==========", "", (%cl.canForceMatchStart && %tModeWaiting && !$loadingMission && $Server::Half != 2), %cl);
	addLine("START GAME", "smatch", (%cl.canForceMatchStart && %tModeWaiting && !$loadingMission && $Server::Half != 2), %cl);
	addLine("==========", "", (%cl.canForceMatchStart && %tModeWaiting && !$loadingMission && $Server::Half != 2), %cl);
	
	if (($FlagIsDropped[0] || $FlagIsDropped[1]) || ($matchStarted && %curTimePause <= 120 && !$freezedata::actice)) {
		addLine("===================", "", %cl.canPermanentBan, %cl);
		addLine("PAUSE NOT AVAILABLE", "", %cl.canPermanentBan, %cl);
		addLine("===================", "", %cl.canPermanentBan, %cl);
		
	}
	else {
		if (!$freezedata::actice && $matchStarted && %curTimePause > 120 && !$loadingMission) {
			addLine("==========", "", %cl.canPermanentBan, %cl);
            addLine("PAUSE GAME", "pause", %cl.canPermanentBan, %cl);
			addLine("==========", "", %cl.canPermanentBan, %cl);
		}
		else if ($freezedata::actice && !$loadingMission) {
			addLine("===========", "", %cl.canPermanentBan, %cl);
            addLine("RESUME GAME", "pauseresume", %cl.canPermanentBan, %cl);
			addLine("===========", "", %cl.canPermanentBan, %cl);
		}
		else {}
	}
}

function processMenuAdminMenu(%cl, %selection)
{
	if(%selection == "pause")
	{
         freeze::start(%cl);
	}
	else if(%selection == "pauseresume")
	{
         freeze::stop(%cl);
	}
	else if(%selection == "changeteams")
	{
        displayMenuChangeTeamsMenu(%cl);
		return;
	}

    else if(%selection == "cffa")
         AActionsetModeFFA(%cl);

    else if(%selection == "ctourney")
         AActionsetModeTourney(%cl);

    else if(%selection == "smatch")
         AActionstartMatch(%cl);

    else if(%selection == "changeMission")
    {
         %cl.madeVote = ""; //for admins initiating mission change votes.
         displayMenuChangeMissionType(%cl, 0);
         return;
    }
    else if(%selection == "ctimelimit")
    {
		 displayMenuChangeTimeLimit(%cl);
         return;
    }
    else if(%selection == "reset")
	{
    	 displayMenuResetServerDefaults(%cl);
    	 return;
    }
    else if(%selection == "takeovermes")
	{
         displayMenuAnnounceServerTakeover(%cl);
    	 return;
	}
	else if(%selection == "etd")
         AActionsetTeamDamageEnable(%cl, true);

    else if(%selection == "dtd")
         AActionsetTeamDamageEnable(%cl, false);

    else if(%selection == "voteOptions")
    {
	     displayMenuVoteMenu(%cl);
		 return;
 	}
	else if(%selection == "serverToggles")
    {
	     displayMenuServerToggles(%cl);
		 return;
 	}

    Game::menuRequest(%cl);
}

function a(){}

//
// -=-=-=-=-= SERVER TOGGLES -=-=-=-=-=-=-=-=-
//

function displayMenuServerToggles(%cl)
{
    %rec = %cl.selClient;
    %recName = Client::getName(%rec);
	
	buildNewMenu("Admin Options", "serverTogglesMenu", %cl);
	
	addLine("Change to FFA Mode", "cffa", (%cl.canChangeGameMode && $Server::TourneyMode && $FFATourney && !$loadingMission), %cl);
	addLine("Change to Tournament Mode", "ctourney", (%cl.canChangeGameMode && !$Server::TourneyMode && $FFATourney && !$loadingMission), %cl);
	
	addLine("Enable Balanced Mode", "yesbalance", (%cl.canChangeGameMode && !$Server::BalancedMode && $Server::Half < 2 && !$loadingMission && $FFATourney && !$Server::disableBalanced), %cl);
	addLine("Disable Balanced Mode", "nobalance", (%cl.canChangeGameMode && $Server::BalancedMode && $Server::Half < 2 && !$loadingMission && $FFATourney && !$Server::disableBalanced), %cl);
	
	addLine("Enable Overtime", "yesovertime", (%cl.canChangeGameMode && !$Game::LT::OvertimeEnabled && !$loadingMission && !$Game::LT::Overtime), %cl);
	addLine("Disable Overtime", "noovertime", (%cl.canChangeGameMode && $Game::LT::OvertimeEnabled && !$loadingMission && !$Game::LT::Overtime), %cl);

	addLine("Enable Anti-Scum", "yesantiscum", (%cl.canChangeGameMode && !$AntiScum::ENABLED), %cl);
	addLine("Disable Anti-Scum", "noantiscum", (%cl.canChangeGameMode && $AntiScum::ENABLED), %cl);
	
	addLine("Back...", "adminmenu", (%cl.adminLevel > 0), %cl);

}

function processMenuServerTogglesMenu(%cl, %sel)
{
	
	if (%sel == "cffa") {
         AActionsetModeFFA(%cl);
	}
	
	else if (%sel == "ctourney") {
         AActionsetModeTourney(%cl);
	}
	 
	if (%sel == "yesbalance") {
		$Server::BalancedMode = $BalanceMode::Option;
		messageAll(0, "Balanced Mode has been ENABLED by an Admin.");
		$Server::Half = 1;
		Server::BalancedModeTime(true);
		
	}
	else if (%sel == "nobalance") {
		$Server::BalancedMode = 0;
		messageAll(0, "Balanced Mode has been DISABLED by an Admin.");
		$Server::Half = 1;
		Server::BalancedModeTime(false);
	}
	
	if (%sel == "yesovertime") {
		$Game::LT::OvertimeEnabled = true;
		messageAll(1, "Overtime has been ENABLED by an Admin.~wLeftMissionArea.wav");
	}
	else if (%sel == "noovertime") {
		$Game::LT::OvertimeEnabled = false;
		messageAll(1, "Overtime has been DISABLED by an Admin.~wLeftMissionArea.wav");
	}

	if (%sel == "yesantiscum") {
		$AntiScum::ENABLED = true;
		messageAll(0, "Anti-scum has been ENABLED by an Admin.");
	}
	else if (%sel == "noantiscum") {
		$AntiScum::ENABLED = false;
		messageAll(0, "Anti-scum has been DISABLED by an Admin.");
	}

	Game::menuRequest(%cl);
}

function displayMenuVoteMenu(%cl)
{
	ObjectiveMission::refreshTeamScores();
	
    %rec = %cl.selClient;
    %recName = Client::getName(%rec);
	%tModeWaiting = $Server::TourneyMode && (!$CountdownStarted && !$matchStarted);

	buildNewMenu("Options", "votemenu", %cl);
	addLine("Change Teams/Observe", "changeteams", (!$freezedata::actice && !$loadingMission && %cl.adminLevel <= 0) && (!$matchStarted || !$Server::TourneyMode), %cl);
	addLine("Vote to change mission", "vChangeMission", !$freezedata::actice && !$loadingMission, %cl);
	addLine("Vote to enter FFA mode", "vcffa", (!$freezedata::actice && $Server::TourneyMode && $FFATourney && !$loadingMission), %cl);
	addLine("Vote to start the match", "vsmatch", (%tModeWaiting && !$loadingMission), %cl);
	addLine("Vote to enter Tournament mode", "vctourney", (!$freezedata::actice && !$Server::TourneyMode && $FFATourney && !$loadingMission), %cl);
	addLine("Back...", "adminoptions", (%cl.adminLevel > 0 && !$loadingMission), %cl);
}

function processMenuVoteMenu(%cl, %selection)
{
	if(%selection == "changeteams")
	{
         displayMenuChangeTeamsMenu(%cl);
		 return;
	}
    else if(%selection == "vsmatch")
         AActionstartVote(%cl, "start the match", "smatch", 0);
    else if(%selection == "vetd")
         AActionstartVote(%cl, "enable team damage", "etd", 0);
    else if(%selection == "vdtd")
         AActionstartVote(%cl, "disable team damage", "dtd", 0);
    else if(%selection == "etd")
         AActionsetTeamDamageEnable(%cl, true);
    else if(%selection == "dtd")
         AActionsetTeamDamageEnable(%cl, false);
    else if(%selection == "vcffa")
         AActionstartVote(%cl, "change to Free For All mode", "ffa", 0);
    else if(%selection == "vctourney")
         AActionstartVote(%cl, "change to Tournament mode", "tourney", 0);
    else if(%selection == "vChangeMission")
    {
         %cl.madeVote = true;
         displayMenuChangeMissionType(%cl, 0);
         return;
    }
	else if(%selection == "adminoptions")
	{
	   //no need to add, falls through to Game::menu request anyway
    }
	Game::menuRequest(%cl);
}

//
// -=-=-=-=-= SERVER TOGGLES -=-=-=-=-=-=-=-=-
//


function a(){}

function displayMenuVotePendingMenu(%cl)
{
    buildNewMenu("Vote in progress", "votePendingMenu", %cl);

	addLine("Vote YES to " @ $curVoteTopic, "voteYes " @ $curVoteCount, %cl.vote == "", %cl);
	addLine("Vote No to " @ $curVoteTopic, "voteNo " @ $curVoteCount, %cl.vote == "", %cl);
	addLine("VETO Vote to " @ $curVoteTopic, "veto", %cl.canCancelVote, %cl);
	addLine("Admin Options...", "adminoptions", (%cl.adminLevel > 0), %cl);
}

function processMenuVotePendingMenu(%cl, %sel)
{
	%selection = getWord(%sel, 0);
	if(%selection == "voteYes") // && %cl == $curVoteCount)	************************
    {
         %cl.vote = "yes";
         centerprint(%cl, "", 0);
    }
    else if(%selection == "voteNo") // && %cl == $curVoteCount)	*************************
    {
         %cl.vote = "no";
         centerprint(%cl, "", 0);
    }
	else if(%selection == "veto")
	{
	    messageAll(0, "Vote to " @ $curVoteTopic @ " was VETO'd by an Admin.");
		bottomPrintAll("",0);
		$curVoteTopic = "";
      	aActionvoteFailed();
    }
	else if(%selection == "adminoptions")
	{
	   displayMenuAdminMenu(%cl);
	   return;
	}
	Game::menuRequest(%cl);
}

function a(){}

function displayMenuSelfSelMenu(%cl)
{
	buildNewMenu("Options", "selfselmenu", %cl);
	addLine("Change Teams/Observe", "changeteams", (!$loadingMission) && (!$matchStarted || !$Server::TourneyMode), %cl);
	//addLine("Vote to admin yourself", "vadminself", !%cl.adminLevel, %cl);
}

function processMenuSelfSelMenu(%cl, %selection)
{
    if(%selection == "changeteams")
        displayMenuChangeTeamsMenu(%cl);

	else if (%selection == "vadminself")
    {
         %cl.voteTarget = true;
         AActionstartVote(%cl, "admin " @ Client::getName(%cl), "admin", %cl);
		 Game::menuRequest(%cl);
    }
}

function a(){}

function displayMenuNonSelfSelMenu(%cl)
{
	%rec = %cl.selClient;
    %recName = Client::getName(%rec);
	if(%cl.canBan)
	   %kickMsg = "Kick or Ban ";
	else
	   %kickMsg = "Kick ";

	buildNewMenu("Options", "nonselfselmenu", %cl);

	//addLine("Vote to admin " @ %recName, "vadmin " @ %rec, !%cl.canMakeAdmin, %cl);
	addLine("Vote to kick " @ %recName, "vkick " @ %rec, !%cl.canKick, %cl);

	addLine(%kickMsg @ %recName, "kickban " @ %rec, %cl.canKick, %cl);
	addLine("Message " @ %recName, "message " @ %rec, %cl.canSendWarning, %cl);
	addLine("Change " @ %recName @ "'s team", "fteamchange " @ %rec, %cl.canChangePlyrTeam, %cl);
	addLine("Admin " @ %recName, "admin " @ %rec, %cl.canMakeAdmin, %cl);
	addLine("Strip " @ %recName, "stradmin " @ %rec, (%cl.canStripAdmin && %rec.adminLevel > 0), %cl);

    addLine("Observe " @ %recName, "observe " @ %rec, (%cl.observerMode == "observerOrbit"), %cl);

	addLine("UnMute " @ %recName, "unmute " @ %rec, %cl.muted[%rec], %cl);
	addLine("Mute " @ %recName, "mute " @ %rec, !%cl.muted[%rec], %cl);

	addLine("Global UnMute " @ %recName, "gunmute " @ %rec, (%cl.adminLevel >= $minAccessRequired::Mute) && %rec.globalMute && !%rec.megaMute, %cl);
	addLine("Global Mute " @ %recName, "gmute " @ %rec, (%cl.adminLevel >= $minAccessRequired::Mute) && !%rec.globalMute && !%rec.megaMute, %cl);

	addLine("MEGA UnMute " @ %recName, "munmute " @ %rec, (%cl.adminLevel >= $minAccessRequired::Mute) && %rec.megaMute, %cl);
	addLine("MEGA Mute " @ %recName, "mmute " @ %rec, (%cl.adminLevel >= $minAccessRequired::Mute) && !%rec.megaMute, %cl);
}

function processMenuNonSelfSelMenu(%cl, %selection)
{
    %selection = getWord(%selection, 0);
    %vic = %cl.selClient;

	if(%selection == "message")
	{
	     displayMenuMessagePlayer(%cl, %vic);
		 return;
	}
    else if(%selection == "admin")
	{
    	 displayMenuBestowAdmin(%cl, %vic);
    	 return;
    }
    else if(%selection == "stradmin")
	{
    	 displayMenuStripAdminship(%cl, %vic);
    	 return;
	}
    else if(%selection == "kickban")
	{
	     displayMenuBanPlayer(%cl, %vic);
    	 return;
	}
	else if(%selection == "fteamchange")
	{
    	 displayMenuForceTeamChange(%cl, %vic);
    	 return;
    }
    else if(%selection == "vkick")
    {
         %vic.voteTarget = true;
         AActionstartVote(%cl, "kick " @ Client::getName(%vic), "kick", %vic);
		 Game::menuRequest(%cl);
    }
    else if(%selection == "vadmin")
    {
         %vic.voteTarget = true;
         AActionstartVote(%cl, "admin " @ Client::getName(%vic), "admin", %vic);
		 Game::menuRequest(%cl);
    }
    else if(%selection == "observe")
    {
         Observer::setTargetClient(%cl, %vic);
         return;
    }
    else if(%selection == "mute")
         %cl.muted[%vic] = true;
    else if(%selection == "unmute")
         %cl.muted[%vic] = "";
	else if ((%selection == "gmute") && (%cl.adminLevel > %vic.adminLevel))
	{
		if ($zadmin::pref::log::Mute)
			logEntry(%cl, "Global Muted", %vic);

		%vic.globalMute = true;
	}
	else if ((%selection == "mmute") && (%cl.adminLevel > %vic.adminLevel))
	{
		if ($zadmin::pref::log::Mute)
			logEntry(%cl, "MEGA Muted", %vic);

		%vic.globalMute = true;
		%vic.megaMute = true;
	}
	else if ((%selection == "gunmute") && (%cl.adminLevel > %vic.adminLevel))
	{
		if ($zadmin::pref::log::Mute)
			logEntry(%cl, "Global Un-Muted", %vic);

		%vic.globalMute = false;
	}
	else if ((%selection == "munmute") && (%cl.adminLevel > %vic.adminLevel))
	{
		if ($zadmin::pref::log::Mute)
			logEntry(%cl, "MEGA Un-Muted", %vic);

		%vic.megaMute = false;
		%vic.globalMute = false;
	}

    Game::menuRequest(%cl);
}

function a(){}

function displayMenuForceTeamChange(%cl, %vic)
{
    %cl.ptc = %vic;
	buildNewMenu("Force Team Change", "forceTeamChange", %cl);

    addLine("Observer", -2, true, %cl);
	addLine("Automatic", -1, true, %cl);

	for(%i = 0; %i < getNumTeams(); %i++)
	   	addLine(getTeamName(%i), %i, true, %cl);

}

function processMenuForceTeamChange(%clientId, %team)
{
    if(%clientId.canChangePlyrTeam && %clientId.adminlevel >= %clientId.ptc.adminLevel)
    {
         processMenuChangeTeamsMenu(%clientId.ptc, %team, %clientId);
	     //if($logTeamChanges) logEntry(%clientId, "Team Changed", %clientId.ptc);
    }
    %clientId.ptc = "";
}

function a(){}

function displayMenuChangeTeamsMenu(%cl, %opt)
{
    buildNewMenu("Change Teams", "changeTeamsMenu", %cl);

	addLine("Observer", -2, true, %cl);
	addLine("Automatic", -1, true, %cl);

	for(%i = 0; %i < getNumTeams(); %i++)
   		addLine(getTeamName(%i), %i, true, %cl);
}

function processMenuChangeTeamsMenu(%clientId, %team, %adminClient)
{
	if($freezedata::actice && !Observer::isObserver(%clientId))
		return;
	if ($loadMission)
		return;

	checkPlayerCash(%clientId);

    if ( %team != -1 && %team == Client::getTeam(%clientId) || %team >= getNumTeams( ) )
         return;
    %clientTeam = Client::getTeam(%clientId);

    if(%clientId.observerMode == "justJoined")
    {
         %clientId.observerMode = "";
         centerprint(%clientId, "");
    }

	if((!$matchStarted || !$Server::TourneyMode || %adminClient) && %team == -2)
	{
		if(Observer::enterObserverMode(%clientId))
		{
			%clientId.notready = "";

			if(%adminClient == "")
				messageAll(0, Client::getName(%clientId) @ " became an observer.");
			else
				messageAll(0, Client::getName(%clientId) @ " was forced into observer mode by " @ Client::getName(%adminClient) @ ".");

			Game::resetScores(%clientId);
			Game::refreshClientScore(%clientId);
			//ObjectiveMission::refreshTeamScores();
		}
		return;
	}

	//automatic team
	if (%team == -1)
	{
		Game::assignClientTeam(%clientId);
		%team = Client::getTeam(%clientId);
		//ObjectiveMission::refreshTeamScores();
		if (%team == %clientTeam)
			return;
	}

    %player = Client::getOwnedObject(%clientId);

	if(%player != -1 && getObjectType(%player) == "Player" && !Player::isDead(%player))
	{
		playNextAnim(%clientId);
		Player::kill(%clientId);
	}
    %clientId.observerMode = "";

    if(%adminClient == "")
         messageAll(0, Client::getName(%clientId) @ " changed teams.");
    else
         messageAll(0, Client::getName(%clientId) @ " was teamchanged by " @ Client::getName(%adminClient) @ ".");

	//echo("setting team to team #" @ %team);
    GameBase::setTeam(%clientId, %team);
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
         bottomprint(%clientId, "<f1><jc>Press FIRE when ready.", 0);
         %clientId.notready = true;
    }

    //ObjectiveMission::refreshTeamScores();
}

function a(){}

function displayMenuBanPlayer(%clientId, %vic)
{
    buildNewMenu("Boot " @ Client::getName(%vic), "banPlayer", %clientId);

	addLine("Kick " @ Client::getName(%vic), "kick " @ %vic, %clientId.canKick, %clientId);
	addLine("Ban " @ Client::getName(%vic), "ban " @ %vic, %clientId.canBan, %clientId);
	addLine("PermBan " @ parseIP(%vic, 4, 18, true), "fullIP " @ %vic, %clientId.canPermanentBan, %clientId);
	addLine("PermBan " @ parseIP(%vic, 3, 14, true), "threeOctet " @ %vic, %clientId.canPermanentBan, %clientId);
	addLine("PermBan " @ parseIP(%vic, 2, 10, true), "twoOctet " @ %vic, %clientId.canPermanentBan, %clientId);
	addLine("Cancel ", "cancel " @ %vic, true, %clientId);
}

function processMenuBanPlayer(%clientId, %opt)
{
	%action = getWord(%opt, 0);
	%vic = getWord(%opt, 1);

	if (%action == "cancel")
	{
	   Game::menuRequest(%clientId);
	   return;
	}

	buildNewMenu("Boot " @ Client::getName(%vic) @ ", you sure?", "banAffirm", %clientId);

	addLine("Kick " @ Client::getName(%vic), %opt @ " yes", %action == "kick", %clientId);
	addLine("Ban " @ Client::getName(%vic), %opt @ " yes", %action == "ban", %clientId);
	addLine("PermBan " @ parseIP(%vic, 4, 18, true), %opt @ " yes", %action == "fullIP", %clientId);
	addLine("PermBan " @ parseIP(%vic, 3, 14, true), %opt @ " yes", %action == "threeOctet", %clientId);
	addLine("PermBan " @ parseIP(%vic, 2, 10, true), %opt @ " yes", %action == "twoOctet", %clientId);
	addLine("Cancel ", %opt @ " cancel", true, %clientId);
}

function processMenuBanAffirm(%clientId, %opt)
{
    %action = getWord(%opt, 0);
    %recipient = getWord(%opt, 1);
	%affirm = getWord(%opt, 2);

	if (%affirm == "yes")
	{
	    if (%action == "kick")
			AActionkick(%clientId, getWord(%opt, 1), false);
	    else if(%action == "ban")
	        AActionkick(%clientId, getWord(%opt, 1), true);
	    else if (%action == "fullIP")
	   	    permaBan(%clientId, %recipient, 4, 18, false);
	    else if (%action == "threeOctet")
	        permaBan(%clientId, %recipient, 3, 14, false);
		else if (%action == "twoOctet")
		    permaBan(%clientId, %recipient, 2, 10, false);
	}

    Game::menuRequest(%clientId);
}

function a(){}

function displayMenuStripAdminship(%cl, %vic)
{
	buildNewMenu("Strip Adminship", "stripAdminship", %cl);

	addLine("Strip " @ Client::getName(%vic), "strip " @ %vic, true, %cl);
	addLine("Cancel", "no", true, %cl);
}

function processMenuStripAdminship(%clientId, %opt)
{
	%action = getWord(%opt, 0);
    %cl = getWord(%opt, 1);

    if(%action == "strip")
    {
       if (%clientId.adminLevel > %cl.adminLevel)
       {
           //%cl.adminLevel = getAdminLevel("Player");
           %cl.adminLevel = 0;
	       awardAdminship(%cl);
		   if ($zadmin::pref::log::AdminStrip) logEntry(%clientId, "Stripped Admin from", %cl);

	       %cl.registeredName = "Stripped by " @ %clientId.registeredName;
	   }
	   else
	   {
		   if($zadmin::pref::log::AdminStrip) logEntry(%clientId, "tried to strip Admin from", %cl);
		   Client::sendMessage(%clientId, $MSGTypeSystem, "You do not have the power to strip " @ Client::getName(%cl) @ ".");
		   Client::sendMessage(%cl, $MSGTypeGame, Client::getName(%clientId) @ " tried to strip your adminship.");
       }
    }
    Game::menuRequest(%clientId);
}

function a(){}

function displayMenuBestowAdmin(%cl, %vic)
{
    buildNewMenu("Bestow Admin", "bestowAdmin", %cl);

	for (%i = 1; (%i < $accessLevel::Count) && (%i < %cl.adminLevel); %i++)
	{
		addLine($accessLevel::[%i] @ " " @ Client::getName(%vic), "admin" @ %i @ " " @ %vic, true, %cl);
	}

	addLine("Cancel ", "cancel " @ %vic, true, %cl);
}

function processMenuBestowAdmin(%clientId, %opt)
{
   %action = getWord(%opt, 0);
   %cl = getWord(%opt, 1);
   %recipientMessage = "You are now an admin, courtesy of " @ Client::getName(%clientId);
   %adminMessage = "Sent to " @ Client::getName(%cl) @ ": " @ %recipientMessage;

	if (String::FindSubStr(%action, "admin") == 0)
	{
		%adminLevel = String::GetSubStr(%action, 5, 1);

		if ((%clientId.adminLevel > %adminLevel) && (%cl.adminLevel < %adminLevel))
		{
			%cl.adminLevel = %adminLevel;
			%cl.password = "NOPASSWORD";
			awardAdminship(%cl);

			if($zadmin::pref::log::Adminships) logEntry(%clientId, "Adminned", %cl);
			if(%cl != %clientId)
			{
				%adminabbrev = String::getSubStr($accessLevel::[%adminLevel], 0, 1) @ "A";
				%cl.registeredName = %adminabbrev @ "->" @ %clientId.registeredName;

				BottomPrint(%cl, "<jc>" @ %recipientMessage);
				BottomPrint(%clientId, "<jc>" @ %adminMessage);
				Client::sendMessage(%cl, $MSGTypeSystem, %recipientMessage);
			}
		}
	}

   Game::menuRequest(%clientId);
}

function a(){}

function displayMenuMessagePlayer(%cl, %recipient)
{
	buildNewMenu("Message Player", "messagePlayer", %cl);

 	addLine($zadmin::pref::warnings::msg[1], 1 @ " " @ %recipient, $zadmin::pref::warnings::text[1] != "", %cl);
	addLine($zadmin::pref::warnings::msg[2], 2 @ " " @ %recipient, $zadmin::pref::warnings::text[2] != "", %cl);
	addLine($zadmin::pref::warnings::msg[3], 3 @ " " @ %recipient, $zadmin::pref::warnings::text[3] != "", %cl);
	addLine($zadmin::pref::warnings::msg[4], 4 @ " " @ %recipient, $zadmin::pref::warnings::text[4] != "", %cl);
	addLine($zadmin::pref::warnings::msg[5], 5 @ " " @ %recipient, $zadmin::pref::warnings::text[5] != "", %cl);
	addLine($zadmin::pref::warnings::msg[6], 6 @ " " @ %recipient, $zadmin::pref::warnings::text[6] != "", %cl);
	addLine("Cancel", "cancel " @ %recipient, true, %cl);
}

function processMenuMessagePlayer(%cl, %opt)
{
   	%choice = getWord(%opt, 0);
	%selId = getWord(%opt, 1);

	if(%choice == "cancel")
	    return;
	else
	{
	    CenterPrint(%selId, "<jc>" @ $zadmin::pref::warnings::text[%choice]);
		BottomPrint(%cl, "<jc>(Sent to " @ Client::getName(%selId) @ ") " @ $zadmin::pref::warnings::text[%choice]);
		if ($zadmin::pref::log::Warnings) logEntry(%cl, "issued a " @ $zadmin::pref::warnings::msg[%choice] @ " to", %selId);
	}

	Game::menuRequest(%cl);
}

function a(){}

function displayMenuChangeTimeLimit(%cl)
{
	buildNewMenu("Change Time Limit", "changeTimeLimit", %cl);
	
	addLine("5 minutes", 5, true, %cl);
	addLine("10 minutes", 10, true, %cl);
	addLine("15 minutes", 15, true, %cl);
	addLine("20 minutes", 20, true, %cl);
	addLine("25 minutes", 25, true, %cl);
	addLine("30 minutes", 30, true, %cl);
	addLine("45 minutes", 45, true, %cl);
	addLine("60 minutes", 60, true, %cl);
	addLine("No time limit", 0, true, %cl);
}

function processMenuChangeTimeLimit(%cl, %opt)
{
    remoteSetTimeLimit(%cl, %opt);
	Game::checkTimeLimit();
}

function a(){}

function displayMenuResetServerDefaults(%cl)
{
   buildNewMenu("Reset Server Defaults", "resetServerDefaults", %cl);

   addLine("Reset Server Defaults", "yes", true, %cl);
   addLine("Cancel", "cancel", true, %cl);
}

function processMenuResetServerDefaults(%cl, %opt)
{
   if(%opt == "yes")
   {
      //if($logServerResets) logEntry(%cl, "reset server defaults", "");
      messageAll(0, Client::getName(%cl) @ " reset the server to default settings.");
      Server::refreshData();
   }

   Game::menuRequest(%cl);
}

function a(){}

function displayMenuAnnounceServerTakeover(%cl)
{
   	buildNewMenu("Announce Server Takeover", "announceServerTakeover", %cl);

    addLine("Friendly Message", "friendly", true, %cl);
	addLine("Firm Message", "firm", true, %cl);
	addLine("Cancel", "cancel", true, %cl);
}

function processMenuAnnounceServerTakeover(%clientId, %opt)
{
    %mes = getWord(%opt, 0);
	if (%mes == "friendly")
	{
    	CenterPrintAll("<jc>" @ Client::getName(%clientId) @ ": " @ $zadmin::pref::msg::friendlytakeover);
	    if($zadmin::pref::log::Takeovers) logEntry(%clientId, "announced a friendly takeover message", "");
	}
    if (%mes == "firm")
	{
		CenterPrintAll("<jc>" @ Client::getName(%clientId) @ ": " @ $zadmin::pref::msg::firmtakeover);
		if($zadmin::pref::log::Takeovers) logEntry(%clientId, "announced a firm takeover message", "");
	}

    Game::menuRequest(%cl);
}

function a(){}

function displayMenuChangeMissionType(%clientId, %listStart)
{

   buildNewMenu("Pick Mission Type", "changeMissionType", %clientId);

   for (%mTypeIndex = %listStart; %mTypeIndex < $MLIST::TypeCount; %mTypeIndex++)
   {
      if (%lineNum++ > $maxMenuSize)
	  {
	     addLine("More mission types...", "moreTypes " @ %mTypeIndex, true, %clientId);
		 break;
	  }
	  else if ($MLIST::Type[%mTypeIndex] != "Training")
	     addLine($MLIST::Type[%mTypeIndex], %mTypeIndex @ " 0", true, %clientId);
   }
}

function processMenuChangeMissionType(%clientId, %option)
{
    %type = getWord(%option, 0);
    %index = getWord(%option, 1);


    if (%type == "moreTypes")
        displayMenuChangeMissionType(%clientId, %index);
    else
    {
        buildNewMenu("Change Mission", "changeMission", %clientId);

        for(%i = 0; (%misIndex = getWord($MLIST::MissionList[%type], %index + %i)) != -1; %i++)
        {
            if ((%i + 1) > $maxMenuSize)
            {
               	addLine("More missions...", "more " @ %index + %i @ " " @ %type, true, %clientId);
                break;
            }
		    addLine($MLIST::EName[%misIndex], %misIndex @ " " @ %type, true, %clientId);
        }
    }
}

function processMenuChangeMission(%clientId, %option)
{
   if(getWord(%option, 0) == "more")
   {
      %first = getWord(%option, 1);
      %type = getWord(%option, 2);
      processMenuChangeMissionType(%clientId, %type @ " " @ %first);
      return;
   }
   %mi = getWord(%option, 0);
   %mt = getWord(%option, 1);

   %misName = $MLIST::EName[%mi];
   %misType = $MLIST::Type[%mt];

   // verify that this is a valid mission:
   if(%misType == "" || %misType == "Training")
      return;
   for(%i = 0; true; %i++)
   {
      %misIndex = getWord($MLIST::MissionList[%mt], %i);
      if(%misIndex == %mi)
         break;
      if(%misIndex == -1)
         return;
   }
   if(%clientId.canChangeMission && !%clientId.madeVote)
   {
      if($zadmin::pref::log::MissionChanges)
      	logEntry(%clientId, "changed mission to " @ %misName, "");

      messageAll(0, Client::getName(%clientId) @ " changed the mission to " @ %misName @ " (" @ %misType @ ")");
	  Vote::changeMission();
      Server::loadMission(%misName);
   }
   else
   {
      %clientId.madeVote = "";
      aActionStartVote(%clientId, "change the mission to " @ %misName @ " (" @ %misType @ ")", "changeMission", %misName);
      Game::menuRequest(%clientId);
   }
}

function a(){}

function aActioncountVotes(%curVote)
{

   if(%curVote != $curVoteCount)
      return;

   if($curVoteTopic == "")
      return;

   %votesFor = 0;
   %votesAgainst = 0;
   %votesAbstain = 0;
   %totalClients = 0;
   %totalVotes = 0;
   for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
   {
      %totalClients++;
      if(%cl.vote == "yes")
      {
         %votesFor++;
         %totalVotes++;
      }
      else if(%cl.vote == "no")
      {
         %votesAgainst++;
         %totalVotes++;
      }
      else
         %votesAbstain++;
   }
   %minVotes = floor($Server::MinVotesPct * %totalClients);
   if(%minVotes < $Server::MinVotes)
      %minVotes = $Server::MinVotes;

   if(%totalVotes < %minVotes)
   {
      %votesAgainst += %minVotes - %totalVotes;
      %totalVotes = %minVotes;
   }
   %margin = $Server::VoteWinMargin;
   if($curVoteAction == "admin")
   {
      %margin = $Server::VoteAdminWinMargin;
      %totalVotes = %votesFor + %votesAgainst + %votesAbstain;
      if(%totalVotes < %minVotes)
         %totalVotes = %minVotes;
   }
   if(%votesFor / %totalVotes >= %margin)
   {
      messageAll(0, "Vote to " @ $curVoteTopic @ " passed: " @ %votesFor @ " to " @ %votesAgainst @ " with " @ %totalClients - (%votesFor + %votesAgainst) @ " abstentions.");
      aActionvoteSucceded();
   }
   else  // special team kick option:
   {
      if($curVoteAction == "kick") // check if the team did a majority number on him:
      {
         %votesFor = 0;
         %totalVotes = 0;
         for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
         {
            if(GameBase::getTeam(%cl) == $curVoteOption.kickTeam)
            {
               %totalVotes++;
               if(%cl.vote == "yes")
                  %votesFor++;
            }
         }
         if(%totalVotes >= $Server::MinVotes && %votesFor / %totalVotes >= $Server::VoteWinMargin)
         {
            messageAll(0, "Vote to " @ $curVoteTopic @ " passed: " @ %votesFor @ " to " @ %totalVotes - %votesFor @ ".");
            aActionvoteSucceded();
            $curVoteTopic = "";
            return;
         }
      }
      messageAll(0, "Vote to " @ $curVoteTopic @ " did not pass: " @ %votesFor @ " to " @ %votesAgainst @ " with " @ %totalClients - (%votesFor + %votesAgainst) @ " abstentions.");
      aActionvoteFailed();
   }
   $curVoteTopic = "";
   $simVoteBegin = 0;
}

function aActionStartVote(%clientId, %topic, %action, %option)
{
   if(%clientId.lastVoteTime == "")
      %clientId.lastVoteTime = -$Server::MinVoteTime;

   // we want an absolute time here.
   %time = getIntegerTime(true) >> 5;
   %diff = %clientId.lastVoteTime + $Server::MinVoteTime - %time;

   if(%diff > 0)
   {
      Client::sendMessage(%clientId, 0, "You can't start another vote for " @ floor(%diff) @ " seconds.");
      return;
   }
   if($curVoteTopic == "")
   {
	   
	  //in a vote record times
	  $simVoteBegin = getSimTime();
	  $simVoteEnd = $simVoteBegin + $Server::VotingTime;

	  if ($dedicated)
	      echo("VOTE INITIATED: " @ Client::getName(%clientId) @ " initiated a vote to " @ %topic);

      if(%clientId.numFailedVotes)
         %time += %clientId.numFailedVotes * $Server::VoteFailTime;

      %clientId.lastVoteTime = %time;
      $curVoteInitiator = %clientId;
      $curVoteTopic = %topic;
      $curVoteAction = %action;
      $curVoteOption = %option;
      if(%action == "kick")
         $curVoteOption.kickTeam = GameBase::getTeam($curVoteOption);
      $curVoteCount++;
      bottomprintall("<jc><f1>" @ Client::getName(%clientId) @ " <f0>initiated a vote to <f1>" @ $curVoteTopic, 10);
      for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
         %cl.vote = "";
      %clientId.vote = "yes";
      for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
         if(%cl.menuMode == "options")
            Game::menuRequest(%clientId);
      schedule("aActioncountVotes(" @ $curVoteCount @ ", true);", $Server::VotingTime, 35);
   }
   else
   {
      Client::sendMessage(%clientId, 0, "Voting already in progress.");
   }
}

function aActionstartMatch(%admin)
{
   if(%admin == -1 || %admin.canForceMatchStart)
   {
      if(!$CountdownStarted && !$matchStarted)
      {
         if(%admin == -1)
            messageAll(0, "Match start countdown forced by vote.");
         else
            messageAll(0, "Match start countdown forced by " @ Client::getName(%admin));

         Game::ForceTourneyMatchStart();
      }
   }
}

function aActionsetTeamDamageEnable(%admin, %enabled)
{
   if(%admin == -1 || %admin.canSwitchTeamDamage)
   {
      if(%enabled)
      {
         $Server::TeamDamageScale = 1;
         if(%admin == -1)
            messageAll(0, "Team damage set to ENABLED by consensus.");
         else
		 {
            messageAll(0, Client::getName(%admin) @ " ENABLED team damage.");
			if($zadmin::pref::log::TeamDamage) logEntry(%admin, "enabled Team Damage", "");
		 }
      }
      else
      {
         $Server::TeamDamageScale = 0;
         if(%admin == -1)
            messageAll(0, "Team damage set to DISABLED by consensus.");
         else
		 {
            messageAll(0, Client::getName(%admin) @ " DISABLED team damage.");
			if($zadmin::pref::log::TeamDamage) logEntry(%admin, "disabled Team Damage", "");
		 }
      }
   }
}

function aActionkick(%admin, %client, %ban)
{

   if(%admin != %client && (%admin == -1 || %admin.adminLevel))
   {
      if(%ban && !%admin.canBan)
         return;

      if(%ban)
      {
         %word = "banned";
         %cmd = "BAN: ";
		 %desc = " ban ";
      }
      else
      {
         %word = "kicked";
         %cmd = "KICK: ";
		 %desc = " kick ";
      }


      if(%client.adminLevel > 0)
      {
         if(%admin == -1 && %client.adminLevel > getAdminLevel("Public Admin")) //only voted admins can be kicked by vote
		 {
		    messageAll(0, Client::getName(%client) @ "is an admin and can't be " @ %word @ " by vote.");
            return;
         }
         else if (%admin.adminLevel <= %client.adminLevel) //you must be higher level than the other admin to kick/ban him
         {
            Client::sendMessage(%admin, $MSGTypeSystem, "You do not have the power to" @ %desc @ Client::getName(%client)@".");
            Client::sendMessage(%client, $MSGTypeGame, Client::getName(%admin) @ " just tried to" @ %desc @ "you.");
            if($zadmin::pref::log::KickBan) logEntry(%admin, "attempted to" @ %desc, %client);
            return;
		 }
      }


      %ip = Client::getTransportAddress(%client);

      //echo(%cmd @ %admin @ " " @ %client @ " " @ %ip);

      if(%ip == "")
         return;
      if(%ban)
         BanList::add(%ip, $zadmin::pref::time::ban);
      else
         BanList::add(%ip, $zadmin::pref::time::Kick);

      %name = Client::getName(%client);

      if ($zadmin::pref::log::KickBan && %word == "kicked") logEntry(%admin, %word, %client);
	  if ($zadmin::pref::log::KickBan && %word == "banned") logEntry(%admin, %word, %client, "@");

      if(%admin == -1)
      {
         MessageAll(0, %name @ " was " @ %word @ " from vote.");
         Net::kick(%client, "You were " @ %word @ " by  consensus.");
      }
      else
      {
         MessageAll(0, %name @ " was " @ %word @ " by " @ Client::getName(%admin) @ ".");
         Net::kick(%client, "You were " @ %word @ " by " @ Client::getName(%admin));
      }
   }
}

function aActionsetModeFFA(%clientId)
{
   if($Server::TourneyMode && (%clientId == -1 || %clientId.canChangeGameMode))
   {
      //$Server::TeamDamageScale = 0;
      if(%clientId == -1)
         messageAll(0, "Server switched to Free-For-All Mode.");
      else
	  {
         messageAll(0, "Server switched to Free-For-All Mode by " @ Client::getName(%clientId) @ ".");
		 if($zadmin::pref::log::GameModeChanges) logEntry(%clientId, "switched to FFA Mode.", "");
      }
	  
      $Server::TourneyMode = false;
	  $Server::Half = 1;
      centerprintall(); // clear the messages
	  Server::BalancedModeTime(false);
	  
      if(!$matchStarted && !$countdownStarted)
      {
         if($Server::warmupTime)
            Server::Countdown($Server::warmupTime);
         else
            Game::startMatch();
      }
   }
}

function aActionsetModeTourney(%clientId)
{
   if(!$Server::TourneyMode && (%clientId == -1 || %clientId.canChangeGameMode))
   {
      $Server::TeamDamageScale = 1;
      if(%clientId == -1)
         messageAll(0, "Server switched to Tournament Mode.");
      else
	  {
         messageAll(0, "Server switched to Tournament Mode by " @ Client::getName(%clientId) @ ".");
		 if($zadmin::pref::log::GameModeChanges) logEntry(%clientId, "switched to Tournament Mode.", "");

      }
	  
		$Server::TourneyMode = true;
		$Server::manualTourney = true;
		$Server::Half = 0;
		Server::nextMission(true);
   }
}

function aActionvoteFailed()
{
   $curVoteInitiator.numVotesFailed++;

   if($curVoteAction == "kick" || $curVoteAction == "admin")
      $curVoteOption.voteTarget = "";
}

function aActionvoteSucceded()
{
   $curVoteInitiator.numVotesFailed = "";
   if($curVoteAction == "kick")
   {
      if($curVoteOption.voteTarget)
         aActionkick(-1, $curVoteOption);
   }
   else if($curVoteAction == "admin")
   {
      if($curVoteOption.voteTarget)
      {
		 if($zadmin::pref::log::Adminships) logEntry(-1, "adminned", $curVoteOption);
		 $curVoteOption.adminLevel = getAdminLevel("Public Admin");
		 $curVoteOption.registeredName = "Admin by vote";
		 awardAdminship($curVoteOption);

         messageAll(0, Client::getName($curVoteOption) @ " has become an administrator.");
         if($curVoteOption.menuMode == "options")
            Game::menuRequest($curVoteOption);
      }
      $curVoteOption.voteTarget = false;
   }
   else if($curVoteAction == "changeMission")
   {
      messageAll(0, "Changing to mission " @ $curVoteOption @ ".");
	  Vote::changeMission();
      Server::loadMission($curVoteOption);
   }
   else if($curVoteAction == "tourney")
      aActionsetModeTourney(-1);
   else if($curVoteAction == "ffa")
      aActionsetModeFFA(-1);
   else if($curVoteAction == "etd")
      aActionsetTeamDamageEnable(-1, true);
   else if($curVoteAction == "dtd")
      aActionsetTeamDamageEnable(-1, false);
   else if($curVoteOption == "smatch")
      aActionstartMatch(-1);
}

function remoteAdminPassword(%client, %password)
{
	%oldLevel = %client.adminLevel;

    if ($zadmin::admins[%password] != "")
    {
    	%client.adminLevel = $zadmin::admins[%password, level];
    	%client.registeredName = $zadmin::admins[%password, name];
	}
	else
	{
		%client.registeredName = "";
		%client.adminLevel = 0;
		awardAdminship(%client);
		return;
	 }

	 %client.password =	%password;
	 schedule("testAdminDuplication(" @ %client @ ");", 5);  //wait 5 seconds so we don't override the "has logged in" message sent to Uadmins.
	 awardAdminShip(%client);

	 if (%client.canSeePlayerlist)
	    LP(%client); //spam client's console with player info

     if (%oldLevel != %client.adminLevel) //allow admin to relogin to see LP list without broadcasting alert or logging
	 {
	    inGameAlert(Client::getName(%client) @ " has logged in as " @ $accessLevel::[%client.adminLevel] @ " using " @ %client.registeredName @ "\'s password.");
	    if ($zadmin::pref::log::AdminLogins)
	       logEntry(%client, "activated his/her " @ $accessLevel::[%client.adminLevel] @ " account.", "", "+");
	 }
     %client.tries++;
     if(%client.tries > 5)
     {
	if(%client.gone)
	{
		return;
	}
	%name = client::getName(%client);
	Admin::Exploit(%client, "SAD() Password Spam");
	banlist::add(client::getTransportAddress(%client), 300);
	messageall(0, %name@" Was Kicked For Spamming Admin Passwords");
	kick(%client, "You Were Kicked For Spamming Admin Passwords");
	%client.gone = true;
	return;
     }
     schedule(%client@".tries = 0;", 0.5);
}

function testAdminDuplication(%cl)
{
	%numClients = getNumClients();
	%violatorIndex = 0;
	%violatorList[%violatorIndex] = %cl;
	for (%clientIndex = 0; %clientIndex < %numClients; %clientIndex++)
	{
	   	%otherClient = getClientByIndex(%clientIndex);
		if (%cl != %otherClient && %cl.password == %otherClient.password)
		{
		   %duplicate = true;
		   %violatorList[%violatorIndex++] = %otherClient;
		}
	}
	if(%duplicate)
	{
	   $Alert = %cl.registeredName @ "\'s password is in use by : " @ Client::getName(%cl);
	   for (%vio = 1; %vio <= %violatorIndex; %vio++)
	      $Alert = $Alert @ " & " @ Client::getName(%violatorList[%vio]);
	   //export("Alert", "config\\" @ $zAdminLogFile, true);
	   inGameAlert($alert);
	}
}

function inGameAlert(%message)
{
    %numClients = getNumClients();
	for(%adminIndex = 0; %adminIndex < %numClients; %adminIndex++)
	{
       %admin = getClientByIndex(%adminIndex);
	   if(%admin.canReceiveAlerts)
	      BottomPrint(%admin, "<jc>" @ %message);
    }
}

function permaBan(%admin, %bannedClient, %numWords, %stringSize)
{
    if(!%admin.canPermanentBan)
	   return;

    if (%admin.adminLevel <= %bannedClient.adminLevel) //you must be higher level than the other admin to kick/ban him
    {
         Client::sendMessage(%admin, $MSGTypeSystem, "You do not have the power to ban " @ Client::getName(%bannedClient) @ ".");
         Client::sendMessage(%bannedClient, $MSGTypeGame, Client::getName(%admin) @ " just tried to ban you.");
         if($zadmin::pref::log::KickBan) logEntry(%admin, "attempted to ban ", %bannedClient);
         return;
	}

	%word = 0;
	%charIndex = 0;

    %ip = Client::getTransportAddress(%bannedClient);

	if (String::findSubStr(%ip, "IPX") != -1 || String::findSubStr(%ip, "LOOPBACK") != -1)
	    return; //don't deal with IPX or Loopbacks right now.

	%truncatedIP = parseIP(%bannedClient, %numWords, %stringSize, false);

	$IPBan[$IPBanCount++] = format(%truncatedIP, 20) @ format (%ip, 26) @ Client::getName(%bannedClient) @ " permanently banned by " @ %admin.registeredName @ ".";

	logEntry(%admin, "permanently banned", %bannedClient, "@");
    //export("IPBan" @ $IPBanCount, "config\\" @ $zAdminBanLogFile, true);
	MessageAll(0, Client::getName(%bannedClient) @ " was banned by " @ Client::getName(%admin));

    Net::kick(%bannedClient, $permaBanMessage);
    BanList::addAbsolute();
    BanList::add(%ip, $zadmin::pref::time::ban);
}

function parseIP(%clientId, %numWords, %stringSize, %fillEmptySlots)
{
    %ip = Client::getTransportAddress(%clientId);

	%word = 0;
	%charIndex = 0;

	if (String::findSubStr(%ip, "IPX") != -1 || String::findSubStr(%ip, "LOOPBACK") != -1)
	   return; //don't deal with IPX or Loopbacks

	%formattedIP ="";

    while (%word <= %numWords && %charIndex <= %stringSize)
    {
       %char = String::getSubStr(%ip, %charIndex, 1);

	   if(String::iCompare(%char, ".") ==0 || String::iCompare(%char, ":") == 0)
	       %word++;

       %charIndex++;
       %formattedIP = %formattedIP @ %char;
    }

	if (%fillEmptySlots)
		for (%append = 0; %append <= 4 - %word; %append++)
		{
	    	%formattedIP = %formattedIP @ "xxx";
	    	if (%append < (4 - %word))
	    	    %formattedIP = %formattedIP @ ".";
		}
    return %formattedIP;
}

function onPermaBanList(%clientId)
{
	%match = false;

	%ip = Client::getTransportAddress(%clientId);

    for (%index = 1; %index <= $IPBanCount; %index++)
	{
	   %loggedIP = getWord($IPBan[%index], 0);
	   if (%loggedIP != "" && (!String::nCompare(%ip, %loggedIP, getLength(%loggedIP))))
	   {
	      echo("$IPBan" @ %index @ " causes this player to be banned.");
	      %match = true;
	   }
	}
	return %match;
}




function kickBanned(%cl)
{
    logEntry(-2, "automatically re-banned", %cl, "!");
	echo("AUTOBOOT: " @ Client::getName(%cl) @ " has been previously permabanned and is being dropped.");
    %ip = Client::getTransportAddress(%cl);
	BanList::add(%ip, $zadmin::pref::time::ban);
	Net::Kick(%cl, $zadmin::pref::msg::permanentban);
}

function resetNumBanEntries()
{
	deleteVariables("$IPBan*");
//   for (%i = 0; %i < 1000; %i++)
//      $IPBan[%i] = "";

   exec($zAdminBanLogFile);

	for (%i = 0; %i < 1000; %i++)
	{
		if ($IPBan[%i] != "")
			$IPBanCount = %i;
	}

//   %entryNum = 0;
//   while ($IPBan[%entryNum++] != "")
//      $IPBanCount = %entryNum;
}

function BANEXCLUSIONS()
{
     //dummy for text editor.
}

function BanExclusions::refresh()
{
	%i = 0;
	while($exclusionList[%i, 0])
	{
	     $exclusionList[%i, 0] = "";
		 $exclusionList[%i, 1] = "";
		 $exclusionList[%i, 2] = "";
		 $exclusionList[%i, 3] = "";
		 $exclusionList[%i, 4] = "";
		 $exclusionList[%i, 5] = "";
		 %i++;
    }

	$Exclusions = 0;
	exec($zAdminBanExclusionsFile);
}

function BanExclusions::add(%ip, %smurf1, %smurf2, %smurf3, %smurf4, %smurf5)
{
    $Exclusions++;
    $exclusionList[$Exclusions, 0] = %ip;
	$exclusionList[$Exclusions, 1] = %smurf1;
	$exclusionList[$Exclusions, 2] = %smurf2;
	$exclusionList[$Exclusions, 3] = %smurf3;
	$exclusionList[$Exclusions, 4] = %smurf4;
	$exclusionList[$Exclusions, 5] = %smurf5;
}

function BanExclusions::isMember(%cl)
{
	%loginIP = Client::getTransportAddress(%cl);

	for (%i = 1; %i <= $Exclusions; %i++)
	{
	    %excludedIP	 = $ExclusionList[%i, 0];
		echo("...comparing " @ %excludedIP @ " with " @ %loginIP);
		if (%excludedIP != "" && (!String::nCompare(%loginIP, %excludedIP, getLength(%excludedIP))))
		{
		    %smurfIndex = 0;
		    %loginName = Client::getName(%cl);
		    while($ExclusionList[%i, %smurfIndex++] != "")
		    {
				%smurf = $ExclusionList[%i, %smurfIndex];
				echo("...comparing " @ %smurf @ " with " @ %loginName);
				if(!String::nCompare(%loginName, %smurf, getLength(%loginName)))
				{
				     echo("matches an exclusion list entry - he's in!");
				     return true;
			    }
            }
        }
    }
	echo("compared against all exclusion entries - no match.  Bye bye!");
	return false;
}

function LP(%requester)
{
   if(%requester)
       Client::sendMessage(%requester, $MSGTypeCommand, "________________________________________________________________________");
   else
   	   echo("________________________________________________________");


   for (%i = 0; %i < getNumClients(); %i++)
    {
       %cl = getClientByIndex(%i);
	   if (%cl.adminLevel < 1)
	   {
	      %admin = "##";
		  %smurf = "";
	   }
	   else
	   {
	      %admin = String::getSubStr($accessLevel::[%cl.adminLevel], 0, 1) @ "A";
		  %smurf = "/" @ %cl.registeredName;
	   }

       %clId = format(%cl, 6);
       %admin = format(%admin, 4);
	   %score = format("Score: " @ %cl.score, 12);
	   %tks = format("TKs: " @ %cl.TKs, 9);
	   %ip = format(parseIP(%cl, 4, 18, false), 19);
	   %name = Client::getName(%cl) @ %smurf;

	   if( %requester)
	   {
	       %clInfo = %clId @ %admin @ %tks @ %score @ %ip @ %name;
	       Client::sendMessage(%requester, $MSGTypeCommand, %clInfo);
	   }
	   else
	   {
	       %clInfo = %admin @ %tks @ %score @ %ip @ %name;
	       echo(%clInfo);
	   }
    }
	if(%requester)
	   Client::sendMessage(%requester, $MSGTypeCommand, "________________________________________________________________________");
	else
	   echo("________________________________________________________");

}

zAdminInit();
resetNumBanEntries();
banExclusions::refresh();
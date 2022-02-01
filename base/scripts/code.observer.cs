$GuiModeCommand    = 2;
$LastControlObject = 0;

function Observer::triggerDown(%client)
{
}

function Observer::orbitObjectDeleted(%cl)
{
}

function Observer::leaveMissionArea(%cl)
{
}

function Observer::enterMissionArea(%cl)
{
}

function Observer::triggerUp(%client)
{
   if(%client.observerMode == "dead")
   {
      if(%client.dieTime + $Server::respawnTime < getSimTime())
      {
         if(Game::playerSpawn(%client, true))
         {
            %client.observerMode = "";
            Observer::checkObserved(%client);
         }
      }
   }
   else if ((%client.observerMode == "observerOrbit") || (%client.observerMode == "observerFirst"))
      Observer::nextObservable(%client);
   else if (%client.observerMode == "observerFlag")
      Observer::nextFlagObs(%client);
   else if(%client.observerMode == "observerFly")
   {
      %camSpawn = Game::pickObserverSpawn(%client);
      Observer::setFlyMode(%client, GameBase::getPosition(%camSpawn), 
	      GameBase::getRotation(%camSpawn), true, true);
   }
   else if(%client.observerMode == "justJoined")
   {
      %client.observerMode = "";
      Game::playerSpawn(%client, false);
   }
   else if(%client.observerMode == "pregame" && $Server::TourneyMode)
   {
      if($CountdownStarted)
         return;

      if(%client.notready)
      {
         %client.notready = "";
         MessageAll(0, Client::getName(%client) @ " is READY.");
         if(%client.notreadyCount < 3)
            bottomprint(%client, "<f1><jc>Waiting for match start (FIRE if not ready).", 0);
         else 
            bottomprint(%client, "<f1><jc>Waiting for match start.", 0);
      }
      else
      {
         %client.notreadyCount++;
         if(%client.notreadyCount < 4)
         {
            %client.notready = true;
            MessageAll(0, Client::getName(%client) @ " is NOT READY.");
            bottomprint(%client, "<f1><jc>Press FIRE when ready.", 0);
         }
         return;
      }
      Game::CheckTourneyMatchStart();
   }
}
function Observer::CheckFlagStatus(%flagTeam)
{
    
    for(%i=0; %i<$Observer::maxObservers; %i++) {
        
        if($ObservingFlag[ $ObserverClient[%i] ] == %flagTeam) {
    
            if($freeze::FlagClient[%flagTeam] == 0) {
            
                Observer::setTargetObject($ObserverClient[%i], $teamFlag[%flagTeam]);
        
            }
            else {
            
                Observer::setTargetObject($ObserverClient[%i], $freeze::FlagClient[%flagTeam]);
            }
        }
    }     
}

function Observer::ConstructObservers()
{
    %i = 0;
    for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
            
            %clTeam = Client::getTeam(%cl);
            
            if( (%clTeam == 0) || (%clTeam == 1) ) {
                
                $ObservingFlag[%cl] = -1;
            }
            else {
                
                $ObserverClient[%i] = %cl;
                %i++;
            }
    }
    
    $Observer::maxObservers = %i;
}

function Observer::nextFlagObs(%client)
{
    if($ObservingFlag[%client] == 0) {
        
        $ObservingFlag[%client] = 1;
        bottomprint(%client, "<jc>DIAMOND SWORD FLAG", 5);
        
    }
    else if($ObservingFlag[%client] == 1) {
        
        $ObservingFlag[%client] = 0;
        bottomprint(%client, "<jc>BLOOD EAGLE FLAG", 5);
    }
    else { return; }
    
    if ($freeze::FlagClient[$ObservingFlag[%client]] == 0) {
        Observer::setTargetObject(%client, $teamFlag[$ObservingFlag[%client]]);
    }
    else {
        Observer::setTargetObject(%client, $freeze::FlagClient[$ObservingFlag[%client]]);
    }
    
}

function Observer::jump(%client)
{
	if(%client.observerMode == "observerFly")
	{
		%client.observerMode = "observerOrbit";
        $ObservingFlag[%client] = -1;
        
		%client.observerTarget = %client;
		Observer::nextObservable(%client);
	}
	else if(%client.observerMode == "observerOrbit")
	{
		%client.observerMode = "observerFirst";
        $ObservingFlag[%client] = -1;

		Observer::setTargetClient(%client, %client.observerTarget);
        
	}
	else if(%client.observerMode == "observerFirst")
	{
        %client.observerMode = "observerFlag";
        $ObservingFlag[%client] = 0;
        
        if ($freeze::FlagClient[0] == 0) {
            Observer::setTargetObject(%client, $teamFlag[0]);
        }
        else {
            Observer::setTargetObject(%client, $freeze::FlagClient[0]);
        }
        bottomprint(%client, "<jc>BLOOD EAGLE FLAG", 5);
	}
    else if (%client.observerMode == "observerFlag") {
        
        %client.observerTarget = "";
		%client.observerMode = "observerFly";
        $ObservingFlag[%client] = -1;

		%camSpawn = Game::pickObserverSpawn(%client);
		Observer::setFlyMode(%client, GameBase::getPosition(%camSpawn), 
		GameBase::getRotation(%camSpawn), true, true);
        bottomprint(%client, "", 1);
        
    }
    else { }
}

function Observer::isObserver(%clientId)
{
   return %clientId.observerMode == "observerOrbit" || %clientId.observerMode == "observerFly" || %clientId.observerMode == "observerFirst" || %clientId.observerMode == "observerFlag";
}

function Observer::enterObserverMode(%clientId)
{
    
   if(%clientId.observerMode == "observerOrbit" || %clientId.observerMode == "observerFly" || %clientId.observerMode == "observerFirst" || %clientId.observerMode == "observerFlag")
      return false;
  
   Client::clearItemShopping(%clientId);
   %player = Client::getOwnedObject(%clientId);
   if(%player != -1 && getObjectType(%player) == "Player" && !Player::isDead(%player)) {
		playNextAnim(%clientId);
	   Player::kill(%clientId);
	}
   Client::setOwnedObject(%clientId, -1);
   Client::setControlObject(%clientId, Client::getObserverCamera(%clientId));
   
   %clientId.observerMode = "observerFlag";
 
   GameBase::setTeam(%clientId, -1);
   Observer::jump(%clientId);
   remotePlayMode(%clientId);
   return true;
}

function Observer::checkObserved(%client)
{
   // this function loops through all the clients and checks
   // if anyone was observing %client... if so, it updates that
   // observation to reflect the new %client owned object.

   for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
   {
      if(%cl.observerTarget == %client)
      {
         if(%cl.observerMode == "observerOrbit")
      	   Observer::setOrbitObject(%cl, %client, 5, 5, 5);
         else if(%cl.observerMode == "observerFirst")
      	   Observer::setOrbitObject(%cl, %client, -1, -1, -1);
         else if(%cl.observerMode == "commander")
   		   Observer::setOrbitObject(%cl, %client, -3, -3, -3);
      }
   }
}

function Observer::setTargetObject(%client, %target)
{
   %owned = %target;
   if(%owned == -1)
      return false;

   
   Observer::setOrbitObject(%client, %target, 5, 5, 5);

   %client.observerTarget = %target;
   return true;
}

function Observer::setTargetClient(%client, %target)
{
	if ((%client.observerMode != "observerOrbit") && (%client.observerMode != "observerFirst"))
		return false;

	%owned = Client::getOwnedObject(%target);
	if(%owned == -1)
		return false;

	if(%client.observerMode == "observerOrbit")
	{
		Observer::setOrbitObject(%client, %target, 5, 5, 5);
		bottomprint(%client, "<jc>Third Person Observing: " @ Client::getName(%target), 5);
	}
	else if(%client.observerMode == "observerFirst")
	{
		Observer::setOrbitObject(%client, %target, -1, -1, -1);
		bottomprint(%client, "<jc>First Person Observing: " @ Client::getName(%target), 5);
	}

	%client.observerTarget = %target;
	return true;
}

function Observer::nextObservable(%client)
{
   %lastObserved = %client.observerTarget;
   %nextObserved = Client::getNext(%lastObserved);
   %ct = 128;  // just in case
   while(%ct--)
   {
      if(%nextObserved == -1)
      {
         %nextObserved = Client::getFirst();
         continue;
      }
      %owned = Client::getOwnedObject(%nextObserved);
      if(%nextObserved == %lastObserved && %owned == -1)
      {
         Observer::jump(%client);
         return;
      }
      if(%owned == -1)
      {
         %nextObserved = Client::getNext(%nextObserved);
         continue;
      }
      Observer::setTargetClient(%client, %nextObserved);
      return;
   }
   Observer::jump(%client);
}

function Observer::prevObservable(%client)
{
}

function remoteSCOM(%clientId, %observeId)
{
   if (%observeId != -1)
   {
      if (Client::getTeam(%clientId) == Client::getTeam(%observeId) &&
         (%clientId.observerMode == "" || %clientId.observerMode == "commander") && Client::getGuiMode(%clientId) == $GuiModeCommand)
      {
         Client::limitCommandBandwidth(%clientId, true);
         if(%clientId.observerMode != "commander")
         {
            %clientId.observerMode = "commander";
	         %clientId.lastControlObject = Client::getControlObject(%clientId);
         }
	      Client::setControlObject(%clientId, Client::getObserverCamera(%clientId));
		   Observer::setOrbitObject(%clientId, %observeId, -3, -3, -3);
         %clientId.observerTarget = %observeId;
         Observer::setDamageObject(%clientId, %clientId);
      }
   }
   else
   {
      Client::limitCommandBandwidth(%clientId, false);
      if(%clientId.observerMode == "commander")
      {
         Client::setControlObject(%clientId, %clientId.lastControlObject);
		   %clientId.lastControlObject = "";
         %clientId.observerMode = "";
         %clientId.observerTarget = "";
	   }
   }
}
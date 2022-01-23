// Try to balance skiing and hiding in LT. Writeup here:
// https://docs.google.com/document/d/1-zY3BeaTFgX6T3mLt1rcWGveGTDnL-IQuNn0AC4ps0g/edit

Attachment::AddAfter("Client::onFlagGrab", "AntiScum::onFlagGrab");
Attachment::AddAfter("Client::onFlagPickup", "AntiScum::onFlagPickup");
Attachment::AddAfter("Client::onFlagCap", "AntiScum::onFlagCap");
Attachment::AddAfter("Client::onFlagDrop", "AntiScum::onFlagDrop");
Attachment::AddAfter("Client::onFlagReturn", "AntiScum::onFlagReturn");
Attachment::AddAfter("Player::onDamage", "AntiScum::OnDamage");
Attachment::AddAfter("Client::onKilled", "AntiScum::onKilled");

// Constants
$AntiScum::ENABLED = true;
$AntiScum::DEBUG = false;
$AntiScum::NON_STANDOFF_DURATION_SECONDS = 30;
$AntiScum::STANDOFF_DURATION_SECONDS = 20;
$AntiScum::SAFE_ZONE_RADIUS_METERS = 160;
$AntiScum::TIME_DAMAGE_RATIO = 0.2;  // 20% of max HP, 5 second death
$AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE = "ENEMY_SAFE_ZONE";
$AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE = "FRIENDLY_SAFE_ZONE";
$AntiScum::FLAG_POSITION_NEUTRAL_ZONE = "NEUTRAL_ZONE";

// Each is indexed by the team of the flag being carried and not the team of the flag carrier
function AntiScum::reset(%team) {
  if (!$AntiScum::ENABLED) {
    return;
  }
  $AntiScum::currentFlagCarrier[%team] = "";
  $AntiScum::flagCarrierTimeLeft[%team] = "";
  $AntiScum::flagCarrierDamageList[%team] = "";
  $AntiScum::flagCarrierDamageListCount[%team] = 0;
  $AntiScum::isFlagStandoff[%team] = false;
  $AntiScum::lastFlagCarrier[%team] = "";
  $AntiScum::lastFlagPosition[%team] = "";
}

AntiScum::reset(0);
AntiScum::reset(1);

// Notify client of how much time is left, also do the damage too
function AntiScum::NotifyTimeLeft(%cl, %timeLeft, %force) {
  if (!$AntiScum::ENABLED) {
    return;
  }

  if (%timeLeft < 0) { %timeLeft = 0; }
  
  if (%timeLeft > 5) {
      
    if (%timeLeft == 25 || %timeLeft == 15 || %timeLeft == 10 || %force) {
      Client::sendMessage(%cl, 1, "You have " @ %timeLeft @ " seconds to bring the flag home.~wshell_click.wav");
    }
    else { }

  } else if (%timeLeft == 5) {
    Client::sendMessage(%cl, 1, "You have " @ %timeLeft @ " seconds to bring the flag home.~wError_Message.wav");
  } else if (%timeLeft > 0 && %timeLeft < 5) {
    Client::sendMessage(%cl, 0, "~wError_Message.wav");
  } else if (%timeLeft <= 0) {

    %player = Client::getOwnedObject(%cl);
    %armor = Player::getArmor(%player);
    %damage = %armor.maxDamage * $AntiScum::TIME_DAMAGE_RATIO;
    %currentDamageLevel = GameBase::getDamageLevel(%player);
    
    Client::sendMessage(%cl, 1, "You are burning up! Bring the flag home!~wError_Message.wav");
    //Client::sendMessage(%cl, 0, "~wError_Message.wav");
    
    Player::setDamageFlash(%player, %damage);
    GameBase::setDamageLevel(%player, %currentDamageLevel + %damage);

    if (Player::isDead(%player)) {
      Player::blowUp(%player);
      
      //random output will be 0-4
      %countMsg = 5;
      %randoMsg = floor(getRandom() * (%countMsg - 0.1));

      //random messages for scum
      if (%randoMsg == 0) {
        messageAll(0, Client::getName(%cl) @ " exploded for being a piece of scum!~wBXplo1.wav");
      }
      else if (%randoMsg == 1) {
        messageAll(0, Client::getName(%cl) @ " was casted into the fire!~wBXplo1.wav");
      }
      else if (%randoMsg == 2) {
        messageAll(0, Client::getName(%cl) @ " was punished for being a scum lord!~wBXplo1.wav");
      }
      else if (%randoMsg == 3) {
        messageAll(0, Client::getName(%cl) @ " is basically a certified moron!~wBXplo1.wav");
      }
      else {
        messageAll(0, Client::getName(%cl) @ " blew up for being scummy!~wBXplo1.wav");
      }
      
      // Not sure what -100 but it's what TR uses
      Client::onKilled(%cl, %cl, -100);
      %clTeam = Client::getTeam(%cl);
      %otherTeam = (%clTeam + 1) % 2;
      $AntiScum::flagCarrierDamageList[%otherTeam] = "";
      $AntiScum::flagCarrierDamageListCount[%otherTeam] = 0;
      $AntiScum::currentFlagCarrier[%otherTeam] = "";
      $AntiScum::lastFlagCarrier[%otherTeam] = "";
    }
  }
}

function AntiScum::echo(%msg) {
  if (!$AntiScum::ENABLED) {
    return;
  }
  if ($AntiScum::DEBUG) {
    echo(%msg);
  }
}

// Return the distance the client is from the team's flag
function AntiScum::getDistanceFromTeamFlag(%cl, %team) {
  if (!$AntiScum::ENABLED) {
    return;
  }
  %flagPos = $teamFlag[%team].originalPosition;
  %flagPosX = getWord(%flagPos, 0);
  %flagPosY = getWord(%flagPos, 1);

  %clPos = GameBase::getPosition(%cl);
  %clPosX = getWord(%clPos, 0);
  %clPosY = getWord(%clPos, 1);

  // Zero out Z-axis so its a circle and not a sphere
  return Vector::getDistance(%flagPosX @ " " @ %flagPosY @ " 0", %clPosX @ " " @ %clPosY @ " 0");
}

// Check the time left on timer, determine if timer should tick, and set standoff states
// Also need to notify the client on state transitions (going from one zone to another)

// Cases:
// 0) client is no longer flag carrier - return early without scheduling

// 1) enemy safe zone -> enemy safe zone - do nothing, schedule again
// 2) enemy safe zone -> neutral zone - notify timer active, schedule again -1 second
// 3) enemy safe zone -> friendly safe zone - notify timer reset, reset standoff timer, set standoff state, schedule again

// 4) neutral zone -> enemy safe zone - notify timer inactive, schedule again
// 5) neutral zone -> neutral zone - continue timer notification, schedule again -1 second
// 6) neutral zone -> friendly safe zone - notify timer reset, reset standoff timer, set standoff state, schedule again

// 7) friendly safe zone -> enemy safe zone - notify timer inactive, schedule again
// 8) friendly safe zone -> neutral zone - notify timer active, schedule again -1 second
// 9) friendly safe zone -> friendly safe zone - do nothing, schedule again

function AntiScum::CheckTime(%cl) {
  if (!$AntiScum::ENABLED) {
    return;
  }

  if ($freezedata::actice == 1) {
    schedule("AntiScum::CheckTime(" @ %cl @ ");", 1);
    return;
  }

  %clTeam = Client::getTeam(%cl);
  %otherTeam = (%clTeam + 1) % 2;
  %timeLeft = $AntiScum::flagCarrierTimeLeft[%otherTeam];

  // Case 0
  if ($AntiScum::currentFlagCarrier[%otherTeam] != %cl) {
    return;
  }

  %distFriendly = AntiScum::getDistanceFromTeamFlag(%cl, %clTeam);
  %distEnemy = AntiScum::getDistanceFromTeamFlag(%cl, %otherTeam);
  if (%distFriendly <= $AntiScum::SAFE_ZONE_RADIUS_METERS) {
    %currentFlagPosition = $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE;
  } else if (%distEnemy <= $AntiScum::SAFE_ZONE_RADIUS_METERS) {
    %currentFlagPosition = $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE;
  } else {
    %currentFlagPosition = $AntiScum::FLAG_POSITION_NEUTRAL_ZONE;
  }

  AntiScum::echo("AntiScum::CheckTime timeLeft: " @ %timeLeft @ ", currentFlagPosition: " @ %currentFlagPosition @ ", distFriendly:" @ %distFriendly @ ", distEnemy:" @ %distEnemy);

  // enemy -> enemy
  if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE) {
  }

  // enemy -> neutral
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_NEUTRAL_ZONE) {
    AntiScum::NotifyTimeLeft(%cl, $AntiScum::flagCarrierTimeLeft[%otherTeam], true);
    $AntiScum::flagCarrierTimeLeft[%otherTeam]--;
  }

  // enemy -> friendly
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE) {
    $AntiScum::isFlagStandoff[%otherTeam] = true;
    Client::sendMessage(%cl, 0, "Flag timer paused - you brought the flag home.~wshell_click.wav");
  }

  // neutral -> enemy
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_NEUTRAL_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE) {
    Client::sendMessage(%cl, 0, "Flag timer paused - you're at the enemy base.~wshell_click.wav");
  }

  // neutral -> neutral
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_NEUTRAL_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_NEUTRAL_ZONE) {
    AntiScum::NotifyTimeLeft(%cl, $AntiScum::flagCarrierTimeLeft[%otherTeam]);
    $AntiScum::flagCarrierTimeLeft[%otherTeam]--;
  }

  // neutral -> friendly
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_NEUTRAL_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE) {
    $AntiScum::isFlagStandoff[%otherTeam] = true;
    Client::sendMessage(%cl, 0, "Flag timer paused - you brought the flag home.~wshell_click.wav");
  }

  // friendly -> enemy
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE) {
    Client::sendMessage(%cl, 0, "Flag timer paused - you're at the enemy base.~wshell_click.wav");
  }

  // friendly -> neutral
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_NEUTRAL_ZONE) {
    AntiScum::NotifyTimeLeft(%cl, $AntiScum::flagCarrierTimeLeft[%otherTeam], true);
    $AntiScum::flagCarrierTimeLeft[%otherTeam]--;
  }

  // friendly -> friendly
  else if ($AntiScum::lastFlagPosition[%otherTeam] == $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE && %currentFlagPosition == $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE) {
    $AntiScum::isFlagStandoff[%otherTeam] = true;
  }

  $AntiScum::lastFlagPosition[%otherTeam] = %currentFlagPosition;
  schedule("AntiScum::CheckTime(" @ %cl @ ");", 1);
  return;
}

function AntiScum::onFlagGrab(%team, %cl) {
  if (!$AntiScum::ENABLED) {
    return;
  }

  $AntiScum::currentFlagCarrier[%team] = %cl;
  $AntiScum::flagCarrierTimeLeft[%team] = $AntiScum::NON_STANDOFF_DURATION_SECONDS;
  $AntiScum::isFlagStandoff[%team] = false;
  $AntiScum::lastFlagPosition[%team] = $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE;

  schedule("AntiScum::CheckTime(" @ %cl @ ");", 1);
}

function AntiScum::onFlagDrop(%team, %cl) {
  if (!$AntiScum::ENABLED) {
    return;
  }

  $AntiScum::currentFlagCarrier[%team] = "";
  $AntiScum::lastFlagCarrier[%team] = %cl;
}

function AntiScum::onFlagCap(%team, %cl) {
  if (!$AntiScum::ENABLED) {
    return;
  }
  AntiScum::reset(%team);
}

function AntiScum::onFlagReturn(%team, %cl) {
  if (!$AntiScum::ENABLED) {
    return;
  }
  AntiScum::reset(%team);
}

// Handle a flag pickup in field.
//
// General responsibility is to set the appropriate timer and then schedule the timer checker.
// Reset stuff when its picked up by a new player.
function AntiScum::onFlagPickup(%team, %cl) {
  if (!$AntiScum::ENABLED) {
    return;
  }
  
  %clTeam = Client::getTeam(%cl);
  %otherTeam = %team;
  %distFriendly = AntiScum::getDistanceFromTeamFlag(%cl, %clTeam);
  %distEnemy = AntiScum::getDistanceFromTeamFlag(%cl, %otherTeam);

  // Calculate current flag location and standoff state
  if (%distFriendly <= $AntiScum::SAFE_ZONE_RADIUS_METERS) {
    $AntiScum::lastFlagPosition[%team] = $AntiScum::FLAG_POSITION_FRIENDLY_SAFE_ZONE;
    $AntiScum::isFlagStandoff[%team] = true;
  } else if (%distEnemy <= $AntiScum::SAFE_ZONE_RADIUS_METERS) {
    $AntiScum::lastFlagPosition[%team] = $AntiScum::FLAG_POSITION_ENEMY_SAFE_ZONE;
  } else {
    $AntiScum::lastFlagPosition[%team] = $AntiScum::FLAG_POSITION_NEUTRAL_ZONE;
  }

  $AntiScum::currentFlagCarrier[%team] = %cl;

  // If picked up by a new player, reset stuff
  if (%cl != $AntiScum::lastFlagCarrier[%team]) {
    $AntiScum::flagCarrierDamageList[%team] = "";
    $AntiScum::flagCarrierDamageListCount[%team] = 0;

    if ($AntiScum::isFlagStandoff[%team]) {
      $AntiScum::flagCarrierTimeLeft[%team] = $AntiScum::STANDOFF_DURATION_SECONDS;
    } else {
      $AntiScum::flagCarrierTimeLeft[%team] = $AntiScum::NON_STANDOFF_DURATION_SECONDS;
    }
  }

  if ($AntiScum::lastFlagPosition[%team] == $AntiScum::FLAG_POSITION_NEUTRAL_ZONE) {
    AntiScum::NotifyTimeLeft(%cl, $AntiScum::flagCarrierTimeLeft[%otherTeam], true);
    $AntiScum::flagCarrierTimeLeft[%otherTeam]--;
  }
  schedule("AntiScum::CheckTime(" @ %cl @ ");", 1);
}
 
// If damage is done by flag carrier, track client id
function AntiScum::onDamage(%this, %type, %value, %pos, %vec, %mom, %vertPos, %quadrant, %object) {
  if (!$AntiScum::ENABLED) {
    return;
  }

    //damage from impact etc.
    if (%type == 0) {
        return;
  }

    %shooterClient = %object;
  %shooterTeam = Client::getTeam(%shooterClient);
    %damagedClient = Player::getClient(%this);
  %damagedTeam = Client::getTeam(%damagedClient);

  // Early return if shooter isn't flag carrier
  if (%shooterClient != $AntiScum::currentFlagCarrier[%damagedTeam]) {
    return;
  }

  // Early return if team damage
  if (%shooterTeam == %damagedTeam) {
    return;
  }

  %count = $AntiScum::flagCarrierDamageListCount[%damagedTeam];
  for (%i = 0; %i < %count; %i++) {
    // Early return if we already recorded damage to this target
    if (%damagedClient == $AntiScum::flagCarrierDamageList[%damagedTeam, %i]) {
      return;
    }
  }

  // Record damage to new target
  $AntiScum::flagCarrierDamageList[%damagedTeam, %count] = %damagedClient;
  $AntiScum::flagCarrierDamageListCount[%damagedTeam]++;
}

// If player was damaged or killed by flag carrier, reset flag carrier's timer
// If flag carrier died, reset flag carrier's timer
function AntiScum::onKilled(%playerId, %killerId, %damageType) {
  if (!$AntiScum::ENABLED) {
    return;
  }
  
  //suicide occured - do not reset timer
  if (%playerId == %killerId) {
      return;
  }

  %killedTeam = Client::getTeam(%playerId);
  %killerTeam = Client::getTeam(%killerId);

  // If the flag carrier was the killer
  %currentFlagCarrier = $AntiScum::currentFlagCarrier[%killedTeam];
  if ($AntiScum::currentFlagCarrier[%killedTeam] == %killerId) {
    if ($AntiScum::isFlagStandoff[%killedTeam]) {
      $AntiScum::flagCarrierTimeLeft[%killedTeam] = $AntiScum::STANDOFF_DURATION_SECONDS;
    } else {
      $AntiScum::flagCarrierTimeLeft[%killedTeam] = $AntiScum::NON_STANDOFF_DURATION_SECONDS;
    }
    Client::sendMessage(%currentFlagCarrier, 1, "Flag time reset. You have " @ $AntiScum::flagCarrierTimeLeft[%killedTeam] @ " seconds to bring the flag home.~wshell_click.wav");
    return;
  }
  // See if the flag carrier damaged the player that just died. If so, reset timer
  else if (%currentFlagCarrier) {
    %count = $AntiScum::flagCarrierDamageListCount[%killedTeam];
    for (%i = 0; %i < %count; %i++) {
      %damagedPlayer = $AntiScum::flagCarrierDamageList[%killedTeam, %i];
      if (%playerId == %damagedPlayer) {
        if ($AntiScum::isFlagStandoff[%killedTeam]) {
          $AntiScum::flagCarrierTimeLeft[%killedTeam] = $AntiScum::STANDOFF_DURATION_SECONDS;
        } else {
          $AntiScum::flagCarrierTimeLeft[%killedTeam] = $AntiScum::NON_STANDOFF_DURATION_SECONDS;
        }
        Client::sendMessage(%currentFlagCarrier, 1, "Flag time reset. You have " @ $AntiScum::flagCarrierTimeLeft[%killedTeam] @ " seconds to bring the flag home.~wshell_click.wav");
        return;
      }
    }
  }
  else { }
  
  if (%killedTeam == %killerTeam) {
      %teamKilled = true;
      //we need to set the opposing team value to make the condition valid
      if (%killedTeam == 1) {
          %newTeam = 0;
      }
      else {
          %newTeam = 1;
      }
  }
  else {
      // no teamkill has occured
      %teamKilled = false;
  }
  
  // See if the flag carrier just died, if so reset stuff

  //death by opponent
  if ($AntiScum::lastFlagCarrier[%killerTeam] == %playerId) {
    $AntiScum::flagCarrierDamageList[%killerTeam] = "";
    $AntiScum::flagCarrierDamageListCount[%killerTeam] = 0;
    $AntiScum::currentFlagCarrier[%killerTeam] = "";
    $AntiScum::lastFlagCarrier[%killerTeam] = "";
  }
  //death by teammate
  else if (%teamKilled && ($AntiScum::lastFlagCarrier[%newTeam] == %playerId)) {
    $AntiScum::flagCarrierDamageList[%newTeam] = "";
    $AntiScum::flagCarrierDamageListCount[%newTeam] = 0;
    $AntiScum::currentFlagCarrier[%newTeam] = "";
    $AntiScum::lastFlagCarrier[%newTeam] = "";
  }
  else { }
}

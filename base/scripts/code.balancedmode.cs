Attachment::AddBefore("Player::onDamage", "BalancedMode::onDamage");
Attachment::AddAfter("Client::onFlagCap", "BalancedMode::CapoutWarning");
//Attachment::AddBefore("Overtime::SetOvertime", "BalancedMode::SetOvertime");
Attachment::AddBefore("ObjectiveMission::CheckScoreLimit", "BalancedMode::ObjectiveMission::CheckScoreLimit");

//function BalancedMode::SetOvertime() {
  //if ($Server::BalancedMode)
    //return "halt";
//}

function BalancedMode::ObjectiveMission::CheckScoreLimit() {
	
	//BALANCE MODE TOGGLES NEED TO CREATE
	// 0 = BALANCE MODE OFF
	// 1 = NEW BALANCE MODE, 2nd half adjusted, 8-7 possible.
	// 2 = OLD BALANCE MODE, 15 min half, 7-7 tie.
	// DISBAND TRUE/FALSE DUTIES

	
	if (!$Server::BalancedMode)
		return;
		
		%totalScore = 0;
        for (%i = 0; %i < getNumTeams(); %i++) {
			//do stuff
            %totalScore += $teamScore[%i];
        }
		
        %scoreLimit = $teamScoreLimit * $Server::Half - $Server::Half;
		
		if (!$Server::TourneyMode && $Server::Half == 1 && %totalScore > %scoreLimit) {
			// if in FFA and admin wants to switch to balanced mode, need to check scores to see if its even possible
			$Server::disableBalanced = true;
		}
		
		
		if (%totalScore == %scoreLimit && $Server::Half == 1) {
			$firstHalfCapped = true;
			Game::HalfTimeNow();
        }
		//this is new balance mode
        else if (%totalScore > %scoreLimit && $Server::Half == 2 && $Server::BalancedMode == 1) {
			ObjectiveMission::missionComplete();
            return "halt";
        }
		//this is old balance mode
		else if (%totalScore == %scoreLimit && $Server::Half == 2 && $Server::BalancedMode == 2) {
			ObjectiveMission::missionComplete();
            return "halt";
		}
		else {
			//do nothing
		}
}

function BalancedMode::onDamage(%this, %type, %value, %pos, %vec, %mom, %vertPos, %quadrant, %object)
{
    // Make players invulnerable during halftime for fun
   if ($Server::Halftime) {
        return "halt";
   }
   // If the match is not started do not allow damage to take place.
    if (!$matchStarted && !$countdownStarted) {
       return "halt";
   }
}

function BalancedMode::CapoutWarning(%team, %cl)
{
  if (!$Server::BalancedMode)
    return;

  %combinedScores = 0;
  for (%i = 0; %i < getNumTeams(); %i++)
  {
    %combinedScores += $teamScore[%i];
    if ($teamScore[%i] == $teamScoreLimit)
      return;
  }

  %warningThreshold = 1;

  // If individual team capout is at 8 then each half caps out at 7 and 14.
  %scoreLimit = $teamScoreLimit * $Server::Half - $Server::Half;
  %difference = %scoreLimit - %combinedScores;

  if (%difference <= %warningThreshold && %difference > 0)
  {
    %caps = " CAPS";
    if (%difference == 1)
      %caps = " CAP";
    if ($Server::Half == 1)
    {
      MessageAll(1, "***** HALF ENDS IN " @ %difference @ %caps @ " *****~wmine_act.wav");
    }
  }
}


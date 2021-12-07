Attachment::AddBefore("ObjectiveMission::CheckScoreLimit", "Overtime::ObjectiveMission::CheckScoreLimit");
Attachment::AddBefore("Game::CheckTimeLimit", "Overtime::Game::CheckTimeLimit");
Attachment::AddAfter("Game::startMatch", "Overtime::Game::StartMatch");

function Overtime::Game::CheckTimeLimit() {
	
  if (!$Game::LT::OvertimeEnabled)
	  return;
	
  if(!$Server::timeLimit)
	  return;

	%curTimeLeft = ($Server::timeLimit * 60) + $missionStartTime - getSimTime();
	if(%curTimeLeft <= 0 && $matchStarted)
		Game::ToggleOvertime();
	
	return;
}

function Overtime::ObjectiveMission::CheckScoreLimit() {
	
	if (!$Game::LT::OvertimeEnabled)
	  return;
  
	// overtime
	if($Game::LT::Overtime && !Game::isTie()) {
	  Game::ToggleOvertime();

	  // end the game
	  ObjectiveMission::RefreshTeamScores();
	  ObjectiveMission::missionComplete();
	  return "halt";
	}
	
}

function Game::ToggleOvertime() {
	
	if (!$Game::LT::OvertimeEnabled)
	  return;
  
	if ($Server::Half == 1)
	  return;

	if (Game::IsTie()) {
		$Game::LT::Overtime = true;
		$Game::LT::ServerTimeLimit = $Server::TimeLimit;
		$Server::timeLimit = 0;
		messageAll(1, "Extending, we're going into overtime!~wmine_act.wav");
	}
	else if ($Game::LT::OverTime) {
		$Game::LT::Overtime = false;
		$Server::timeLimit = $Game::LT::ServerTimeLimit;
	}
	else {
		//do nothing
	}
	
}

function Game::isTie()
{
	// Check for tie
	%tieGame = true;
	if(($teamScore[0] != $teamScore[1]) && $teamScore[0] != "" && $teamScore[1] != "") {
		//simple if team 0 and team 1 no longer have equal scores...
	  	%tieGame = false;
	}
	return %tieGame;
}
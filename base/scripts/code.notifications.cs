// Attachment Plugin Required
////////////////////////////////////////////////////////////////////////////////

Attachment::AddAfter("Game::CheckTimeLimit", "Notifications::CountDown");

$Notifications::curTimeLeft = 0;

function Notifications::halftime() {
$Server::Halftime = true;
Game::DisplayHalfScoreboard();
}

function Notifications::remoteVoteYes(%clientId)
{
  %ret = Vote::GetCount($curVoteCount);
  Notifications::DisplayVotes(getWord(%ret, 0),getWord(%ret, 1),
    getWord(%ret, 2),getWord(%ret, 3));
}

function Notifications::remoteVoteNo(%clientId)
{
  %ret = Vote::GetCount($curVoteCount);
  Notifications::DisplayVotes(getWord(%ret, 0),getWord(%ret, 1),
    getWord(%ret, 2),getWord(%ret, 3));
}

function Vote::GetCount(%curVote)
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
    %votePassed = true;
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
        %votePassed = true;
      }
    }
    %votePassed = false;
  }

  return %votesFor @ " " @ %votesAgainst @ " " @ %totalVotes @ " " @ %votePassed;
}

function Notifications::aActionStartVote(%clientId, %topic, %action, %option)
{
  %numClients = getNumClients();
  %center = $Server::MaxPlayers + 1;
  %size = ($Server::MaxPlayers * 2) + 1;
  %unitSize = %size / %numClients / 2 - 1;
  for(%i = 0; %i <= %size; %i++)
  {
    if(%i == %center)
      %blob = %blob @ "<f0>*<f2>";
    else
      %blob = %blob @ " ";
  }
  %blob = "<f2>-"@%blob@"+";
  bottomprintall("<jc>" @ %blob, 50);
}

function Notifications::DisplayVotes(%votesFor, %votesAgainst, %totalVotes, %votePassed)
{
  %numClients = getNumClients();
  %center = $Server::MaxPlayers + 1;
  %size = ($Server::MaxPlayers * 2) + 1;
  %unitSize = floor((%size / %numClients) - 1);
  echo(%unitSize);
  for(%i = 0; %i <= %size + 1; %i++)
  {
    if(%i == (%center + %unitSize))
    {
      %blob = %blob @ "<f0>*<f2>";
    }
    else
      %blob = %blob @ " ";
  }
  %blob = "<f2>-"@%blob@"+";
  bottomprintall("<jc>" @ %blob, 50);
}

function Notifications::CountDown()
{
    
    // if we are in a countdown to begin the next half, cancel this countdown
    if($Server::Halftime)
        return;
    
    //%curTimeLeft = floor(($Server::timeLimit * 60) + floor($missionStartTime) - floor(getSimTime()));
    //MessageAll(0, "CUR TIME LEFT: " @ $Notifications::curTimeLeft);
    //%curTimeLeft = ($Server::timeLimit * 60) + floor($missionStartTime) - floor(getSimTime());
    if(Game::IsTie() && $Game::LT::OvertimeEnabled) $ae = "[ Overtime Enabled ]";

        if ($Server::Half == 1) {
            $halfType = "First half";
            $ae = "";
        }
        else if ($Server::Half == 2) {
            $halfType = "Second half";
            $ae = "";
         }
         else {
            $halfType = "Match";
         }
         

    if (($Notifications::curTimeLeft <= 180) && ($Notifications::curTimeLeft > 120) && (!$curTimeAdjust)) {
        
        //2 minute warning
        $curTimeAdjust = true;
        %timeUntil2Min = ($Notifications::curTimeLeft - 120); //for example 179 - 120 = 59 seconds until 2 minute mark

        //this essentially bumps the main time check back on course at exactly 2 minutes
        schedule("Game::checkTimeLimit();", %timeUntil2Min);
        schedule("$curTimeAdjust = false;", %timeUntil2Min);

    }

    if ($Notifications::curTimeLeft == 120) {
        //2 minute warning
        MessageAll(1, $halfType @ " ends in 2 minutes. [PAUSE DISABLED]~wmine_act.wav");
        
    }
    
    if ($Notifications::curTimeLeft == 60) {
        //1 minute warning
        MessageAll(1, $halfType @ " ends in 1 minute. " @ $ae @ "~wmine_act.wav");
        
    }
    else if ($Notifications::curTimeLeft == 40) {
        
        //30 second warning
        $Notifications::curTimeLeft = 30;
        schedule("Notifications::CountDown();", 10);
        
    }
    else if ($Notifications::curTimeLeft == 30) {
        
        //30 second warning
        MessageAll(1, $halfType @ " ends in 30 seconds. " @ $ae @ "~wmine_act.wav");
        
    }
    else if($Notifications::curTimeLeft == 20) {

      //15 second warning
      schedule(' MessageAll(1, $halfType @ " ends in 15 seconds. " @ $ae @ "~wmine_act.wav"); ', 5);
      
      //global curTimeLeft is assumed by this function now until the end
      $Notifications::curTimeLeft = 10;
      schedule("Notifications::CountDown();", 10);
      
    }
    else if(($Notifications::curTimeLeft <= 10) && ($Notifications::curTimeLeft > 0)) {
        if($Notifications::curTimeLeft == 10) {
        
            MessageAll(1, $halfType @ " ends in 10 seconds. " @ $ae);
        }
        
        if($Notifications::curTimeLeft == 5) {
        
            MessageAll(1, $halfType @ " ends in 5 seconds. " @ $ae);
        }

        if($Notifications::curTimeLeft % 2 == 0) {
        
            MessageAll(0, "~wbutton5.wav");
        }
        else {
        
            MessageAll(0, "~wbutton4.wav");
        }
        
    $Notifications::curTimeLeft--;
    schedule("Notifications::CountDown();", 1);
  }
  else { }
  
}
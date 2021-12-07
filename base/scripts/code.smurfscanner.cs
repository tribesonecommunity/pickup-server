// Clone Scanner By: KingTomato
// Modified by Lemon

// Attachments
Attachment::AddAfter("Server::onClientConnect", "SmurfScan::Delay");
Attachment::AddBefore("remoteSelectClient", "SmurfScan::SelectClient");
Attachment::AddAfter("displayMenuNonSelfSelMenu", "SmurfScan::DisplaySmurfsMenu");
Attachment::AddBefore("processMenuNonSelfSelMenu", "SmurfScan::ProcessMenuSelection");
Attachment::AddAfter("displayMenuSelfSelMenu", "SmurfScan::DisplaySmurfsMenu");
Attachment::AddBefore("processMenuSelfSelMenu", "SmurfScan::ProcessMenuSelection");


function SmurfScan::SelectClient(%clientId, %selId)
{
  %clientId.selIdIdx = 0;
}

function SmurfScan::ProcessMenuSelection(%cl, %selection)
{
  %selection = getWord(%selection, 0);
  %selId = %cl.selClient;

  if(%selection == "SmurfScan::DisplayTabInfo")
    SmurfScan::DisplayTabInfo(%cl, %selId);

  // required for selfMenuRequest
  if(%selId == %cl.selClient)
    Game::menuRequest(%cl);
}

function SmurfScan::DisplaySmurfsMenu(%cl)
{
  %selId = %cl.selClient;
  %selName = Client::getName(%selId);

  if(%cl.selIdIdx)
    %cont = " ( cont )";

  // when selected, call SmurfScan::DisplayTabInfo - might wanna add a server toggle?
  addLine("View " @ %selName @ " smurfs " @ %cont, "SmurfScan::DisplayTabInfo " @ %selId, true, %cl);
}

function SmurfScan::DisplayTabInfo(%clientId, %selId)
{
  // clear this
  for(%line = 2; %line <= 6; %line++)
    remoteEval(%clientId, "setInfoLine", %line, "");

  // info header
  %line = 1;
  %idxSmurf = 0;
  %break = false;
  %ip = SmurfScan::ParseIP(Client::GetTransportAddress(%selId));
  %address = SmurfScan::RepIp(%ip);
  while(!%break)
  {
    %smurfIdx = $Smurf::List[%address, %idxSmurf += %clientId.selIdIdx];
    while((%idxEnd = String::findSubStr(%smurfIdx, "~"))!= -1)
    {
      %smurf = String::getSubStr(%smurfIdx, 0, %idxEnd);
      %smurfLine = %smurfLine @ %smurf @ ",";

      if(String::Len(%smurfLine) + String::Len(%smurf) >= 50)
      {
        remoteEval(%clientId, "setInfoLine", %line++, %smurfLine);
        %smurfLine = "";
      }

      if((%smurfIdx = String::getSubStr(%smurfIdx, %idxEnd + 1, 160)) == "")
      {
        // new page?
        %clientId.selId = %selId;
        if(!%smurfIdx && ($Smurf::List[%address, %idxSmurf + 1] != ""))
          %clientId.selIdIdx++;
        else
          %clientId.selIdIdx = 0;
        %break = true;
        break;
      }
    }

    // strays
    if(%smurfLine != "")
      remoteEval(%clientId, "setInfoLine", %line+1, %smurfLine);

    // break out
    if($Smurf::List[%address, %idxSmurf] == "")
    {
      // reset
      %clientId.selIdIdx = 0;
      %idxSmurf = 0;
      break;
    }
  }

  remoteEval(%clientId, "setInfoLine", 1, "Smurf Info for " @ Client::getName(%selId) @ %spam @ ":");
}

function SmurfScan::Delay(%clientId) {
  schedule("SmurfScan::OnConnect("@%clientId@");", 10);
}

function SmurfScan::OnConnect(%client)
{
  %name = escapeString(Client::GetName(%client));

  // filter multiple-connections
  %len = String::Len(%name);
  if(String::FindSubStr(%name, ".") == (%len - 2))
    %name = String::getSubStr(0, %len - 2);

  %ip = SmurfScan::ParseIP(Client::GetTransportAddress(%client));
  %address = SmurfScan::RepIp(%ip);
  %fileID = String::getSubStr(%address, 0, 2);
  %file = "Smurf_" @ %fileID @ ".cs";

  // load existing smurfs
  if(isFile("temp\\" @ %file))
    exec(%file);

  %adminMsg = "<jc><f2>Player Info:\n\n<f0>IP:<f1>"@%ip@"\n<f0>Client:<f1> " @ escapeString(Client::GetName(%client)) @ " \n<f0>Smurfs:<f1> ";

  // find current index, and build smurf list
  %idxSmurf = 0;
  while(1)
  {
    %smurfList = %smurfList @ $Smurf::List[%address, %idxSmurf];
    if($Smurf::List[%address, %idxSmurf+1] == "")
      break;
    else
      %idxSmurf++;
  }

  // new entry?
  if(%smurfList == "")
  {
    $Smurf::List[%address, %idxSmurf] = %name @ "~";
    %adminMsg = %adminmsg @ "(First Recorded Join)";
    //export( "$Smurf::List" @ %fileID @ "*", "temp\\Smurf_" @ %fileID @ ".cs", false );
  }
  else
  {
    // duplicate?
    if(String::findSubStr(%smurfList, %name) == -1)
    {
      // clamp globals to 256 bytes -- might want to add NAME SPAMMER CODE ?
      %tmp = $Smurf::List[%address, %idxSmurf] @ %name;
      if(String::Len(%tmp) >= 160)
        %idxSmurf++;

       // add new entry
      $Smurf::List[%address, %idxSmurf] = $Smurf::List[%address, %idxSmurf] @ %name @ "~";
      %smurfList = %smurfList @ %name;
      //export("$Smurf::List" @ %fileID @ "*", "temp\\Smurf_" @ %fileID @ ".cs", false);
    }
  }

  %adminMsg = %adminMsg @ %smurfList;

  echo(%client@" IP: " @ %ip @ " Smurfs: " @ %smurfList);
  %client.smurfs = %smurfList;
  deletevariables("$Smurfs::List" @ %fileID @ "*");
}

function SmurfScan::ParseIP(%ip)
{
  if (String::findSubStr(%ip, "IP:") == 0)
    %ip = String::getSubStr(%ip, 3, 16) @ ":";
  return String::getSubStr(%ip, 0, String::findSubStr(%ip, ":"));
}

function SmurfScan::RepIp(%ip)
{
  %loc  = String::findSubStr(%ip, ".");
  while (%loc != -1) {
    %pre  = String::getSubStr(%ip, 0, %loc);
    %post = String::getSubStr(%ip, %loc + 1, 99);
    %ip = %pre @ "_" @ %post;
    %loc  = String::findSubStr(%ip, ".");
  }
  return %ip;
}
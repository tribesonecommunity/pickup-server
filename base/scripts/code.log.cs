function logEntry(%admin, %action, %recipient, %prefix)
{
   if (%prefix == "")
      %prefix = " ";

   %action = " " @ %action;
   
   if (%admin == -1)
   {
       %adminRealName = format("VOTERS", 23) @ "]";
	   %adminSmurf = " VOTERS";
	   %adminIp = " {XXX.XXX.XXX.XXX:XXXX}";
   }
   else	if (%admin == -2)
   {
       %adminRealName = format("zAdmin", 23) @ "]";
	   %adminSmurf = " zAdmin";
	   %adminIp = " {XXX.XXX.XXX.XXX:XXXX}";
   }
   else      
   {
   	   
	   %adminRealName = format(%admin.registeredName, 23) @ "]";
	   %adminSmurf = " " @ Client::getName(%admin);
	   %adminIp = " {" @ Client::getTransportAddress(%admin) @ "}";
   }

   if (%recipient)
   {
   	  %recipientName =	" " @ Client::getName(%recipient);
      %recipientIp = " {" @ Client::getTransportAddress(%recipient) @ "}";	  
   }

   %date = zadmin::getTimeStamp();

   $zAdminLogEntry = format("[" @ %prefix @ " " @ %adminRealName @ %adminSmurf @ %action @ %recipientName, 100); 
   $zAdminLogEntry = $zAdminLogEntry @ format(":" @ %adminSmurf @ %adminIp, 45); 
   $zAdminLogEntry = $zAdminLogEntry @ format("| " @ %recipientName @ %recipientIP, 45); 
   $zAdminLogEntry = "[" @ %date @ "] " @ $zAdminLogEntry @ " : " @ $MissionName;

   //export("zAdminLogEntry", "config\\" @ $zAdminLogFile, true);
}

function logCheat(%player, %msg)
{
	if (%player)
	{
		%playerName =	" " @ Client::getName(%player);
		%playerIp = " {" @ Client::getTransportAddress(%player) @ "}";	  
	}
	else
	{
		return;
	}

	%date = zadmin::getTimeStamp();

	$zadmin::cheatLine = "Player: " @ %playerName @ %playerIp @ " // " @ %msg @ " // " @ $MissionName;

	//export("zadmin::cheatLine", "config\\" @ $zAdminCheatLogFile, true);
}


function format(%text, %size)
{
   %formattedText = String::getSubStr(%text, 0, %size); //truncate if needed 

   %textLen = getLength(%formattedText);       

   if(%textLen < %size)	//append if needed
      for (%spaces = %textLen; %spaces < %size; %spaces++)
         %formattedText = %formattedText @ " ";
   return %formattedText;
}

function getLength(%text)
{
   while (String::getSubStr(%text, %length, 1) != "") 
      %length++;
   return %length;
}

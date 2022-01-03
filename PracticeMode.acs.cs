editActionMap("playMap.sae");
bindCommand(keyboard0, make, "insert", TO, "remoteEval(2048, StorePosition);");
bindCommand(keyboard0, make, "delete", TO, "remoteEval(2048, RestorePosition);");
bindCommand(keyboard0, make, "home", TO, "remoteEval(2048, RestoreHealth);");
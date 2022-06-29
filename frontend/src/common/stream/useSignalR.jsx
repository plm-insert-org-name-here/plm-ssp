import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { useCallback, useEffect, useRef } from "react";

const useSignalR = ({ url, active, groups, handlers }) => {
    const connectionRef = useRef();

    useEffect(() => {
        if (active && url) {
            const conn = new HubConnectionBuilder()
                .withUrl(url)
                .configureLogging(LogLevel.Warning)
                .build();

            connectionRef.current = conn;
            conn.start().then(() => subscribe());
        }

        return () => {
            unsubscribe();
            connectionRef.current = null;
        };
    }, [url, active]);

    useEffect(() => {
        const conn = connectionRef.current;
        if (!conn) return;

        handlers.forEach(([method, _]) => conn.off(method));
        handlers.forEach(([method, handler]) => conn.on(method, handler));
    }, [handlers]);

    const subscribe = useCallback(() => {
        const conn = connectionRef.current;
        if (!conn) return;

        groups.forEach((g) => conn.invoke("JoinGroup", g));
        handlers.forEach(([method, handler]) => conn.on(method, handler));
    }, [groups, handlers]);

    const unsubscribe = useCallback(() => {
        const conn = connectionRef.current;
        if (!conn || conn.state !== HubConnectionState.Connected) return;

        if (groups && conn) {
            const calls = groups.map((g) => conn.invoke("LeaveGroup", g));
            Promise.all(calls).then(() => conn.stop());
        }
    }, [groups]);

    return null;
};

export default useSignalR;

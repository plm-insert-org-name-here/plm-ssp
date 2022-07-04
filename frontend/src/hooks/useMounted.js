import { useCallback, useEffect, useRef } from "react";

const useMounted = () => {
    const isMounted = useRef(false);

    const get = useCallback(() => isMounted.current, []);

    useEffect(() => {
        isMounted.current = true;

        return () => {
            isMounted.current = false;
        };
    }, []);

    return get;
};

export default useMounted;

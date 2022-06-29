import React, {useEffect, useState} from "react";
import axios from "axios";
import {Routes} from "../../routes";
import useMounted from "../../common/hooks/useMounted";
import {Box, Paper, Tab, Tabs} from "@mui/material";
import Locations from "./Locations";
import Detectors from "./Detectors";

function TabPanel({children, value, index, ...other}) {
    return (
        <Box
            flex={1}
            display={value === index ? "flex" : "none"}
            flexDirection="column"
            {...other}
        >{children}</Box>
    );
}

const Infrastructure = () => {
        const [selectedTab, setSelectedTab] = useState(0);

        const [detectors, setDetectors] = useState([]);
        const [locations, setLocations] = useState([]);
        const isMounted = useMounted();

        useEffect(() => {
            const locationsPromise = axios(Routes.locations);
            const detectorsPromise = axios(Routes.detectors);

            Promise.all([locationsPromise, detectorsPromise])
                .then(([locationsRes, detectorsRes]) => {
                        if (isMounted()) {
                            setLocations(locationsRes.data);
                            setDetectors(detectorsRes.data);
                        }
                    }
                );
        }, []);

        return (
            <Paper elevation={8}
                   sx={{display: 'flex', flexDirection: "column", m: 2, flexGrow: 1}}>
                <Box sx={{borderBottom: 1, borderColor: 'divider'}}>
                    <Tabs value={selectedTab} onChange={(_, v) => setSelectedTab(v)}>
                        <Tab label="Locations"/>
                        <Tab label="Detectors"/>
                    </Tabs>
                </Box>
                <TabPanel value={selectedTab} index={0}>
                    <Locations locations={locations} setLocations={setLocations}/>
                </TabPanel>
                <TabPanel value={selectedTab} index={1}>
                    <Detectors detectors={detectors}/>
                </TabPanel>
            </Paper>
        );
    }
;

export default Infrastructure;

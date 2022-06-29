import { bindPopover } from "material-ui-popup-state/hooks";
import React, { useEffect, useState } from "react";

import { ListItem, Popover } from "@mui/material";
import List from "@mui/material/List";
import ListItemButton from "@mui/material/ListItemButton";
import Typography from "@mui/material/Typography";

import OverflowText from "./OverflowText";
import SimpleSearch from "./SimpleSearch";

// TODO(rg): use OverflowText for displaying items with names too long
const MenuPopup = ({ popupProps, items, handler, width = "25ch", displayProperty = "name" }) => {
    const [filteredItems, setFilteredItems] = useState([]);

    useEffect(() => {
        setFilteredItems(items);
    }, [items]);

    const handleSearch = (v) => {
        setFilteredItems(
            items.filter((i) => i[displayProperty].toLowerCase().includes(v.toLowerCase()))
        );
    };

    return (
        <Popover
            {...bindPopover(popupProps)}
            anchorOrigin={{
                vertical: "top",
                horizontal: "right",
            }}
            transformOrigin={{
                vertical: "bottom",
                horizontal: "left",
            }}
        >
            <SimpleSearch handler={handleSearch} timeout={200} controlSx={{ m: 1, width: width }} />
            <List p={1}>
                {filteredItems.length ? (
                    filteredItems.map((item, i) => (
                        <ListItem key={i} sx={{ py: 0, px: 1 }} disablePadding>
                            <ListItemButton sx={{ width: 0 }} onClick={() => handler(item)}>
                                <OverflowText text={item[displayProperty]} />
                            </ListItemButton>
                        </ListItem>
                    ))
                ) : (
                    <Typography
                        variant="overline"
                        component="div"
                        sx={{ display: "flex", justifyContent: "center" }}
                    >
                        No items
                    </Typography>
                )}
            </List>
        </Popover>
    );
};

export default MenuPopup;

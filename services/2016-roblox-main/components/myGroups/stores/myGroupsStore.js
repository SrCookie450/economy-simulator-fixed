import { useState } from "react";
import { createContainer } from "unstated-next";
import { multiGetGroupIcons } from "../../../services/thumbnails";

const MyGroupsStore = createContainer(() => {
  const [groups, setGroups] = useState(null);
  const [icons, setIcons] = useState(null);

  return {
    groups,
    setGroups: groups => {
      if (groups.length === 0) {
        setGroups(groups);
        return;
      }
      multiGetGroupIcons({
        groupIds: groups.map(v => v.group.id),
      }).then(d => {
        let obj = {};
        for (const item of d) {
          obj[item.targetId] = item.imageUrl
        }
        setIcons(obj)
      })
      setGroups(groups);
    },

    icons,
    setIcons,
  }
});

export default MyGroupsStore;
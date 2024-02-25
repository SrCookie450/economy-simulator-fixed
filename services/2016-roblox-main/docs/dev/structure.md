# Project Structure

```
components/
  {componentName}/
    index.js        - The component itself
    components/     - Optional, components only used by this component
    stores/         - Optional, the containers/stores for the page
    styles/         - Optional, folder for JSS styles shared by components
stores/
  {storeName}.js    - Stores that are shared by more than one page. Should be rarely used.
pages/              - NextJS pages directory
services/
  {serviceName}.js  - Services that connect to the Roblox web api. 
                    The {serviceName} is usually equal to the api 
                    site prefix (e.g. "groups.roblox.com" would be "groups")
public/             - Public files, such as images or non-react js
models/
  {name}.js         - Typedefs
lib/
  {libName}.js      - Libraries that are shared by many components and not react-specified
styles/             - JSS or CSS shared by more than one top-level component
```
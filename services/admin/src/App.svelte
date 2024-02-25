<script lang="ts">
	import { Router, Route, Link } from "svelte-routing";
	export let url = "";
	// import pages
	// @ts-ignore
	import Index from "./pages/Index.svelte";
	import Players from "./pages/Players.svelte";
	import ManageUser from "./pages/ManageUser.svelte";
	// other
	import BanUser from "./pages/BanUser.svelte";
	import MessageUser from "./pages/MessageUser.svelte";
	import Logs from "./pages/Logs.svelte";
	import Memos from "./pages/Memos.svelte";
	import ManageUserBadges from "./pages/ManageUserBadges.svelte";
	import ManageUserRobux from "./pages/ManageUserRobux.svelte";
	import ManageUserInventory from "./pages/ManageUserInventory.svelte";
	import ManageUserAsset from "./pages/ManageUserAsset.svelte";
	import CreatePlayer from "./pages/CreatePlayer.svelte";
	import CreateAssetForItem from "./pages/CreateAssetForItem.svelte";
	import CreateAsset from "./pages/CreateAsset.svelte";
	import CreateAssetVersion from "./pages/CreateAssetVersion.svelte";
	import AssetReRender from "./pages/AssetReRender.svelte";
	import UpdateAssetProduct from "./pages/UpdateAssetProduct.svelte";
	import CreateClothing from "./pages/CreateClothing.svelte";
	import CopyRobloxClothing from "./pages/CopyRobloxClothing.svelte";
	import AssetApproval from "./pages/AssetApproval.svelte";
	import ManageUsernames from "./pages/ManageUsernames.svelte";
	import * as rank from "./stores/rank";
import Lottery from "./pages/Lottery.svelte";
import FeatureFlags from "./pages/FeatureFlags.svelte";
import ManageUserTransactions from "./pages/ManageUserTransactions.svelte";
import UserModerationHistory from "./pages/ViewModerationHistory.svelte";
import Applications from "./pages/Applications.svelte";
import ForumPosts from "./pages/TextModeration.svelte";
import Groups from "./pages/Groups.svelte";
import GameHistory from "./pages/GameHistory.svelte";
import Forums from "./pages/Forums.svelte";
import ManageUserTrades from "./pages/ManageUserTrades.svelte";
import TrackUserAsset from "./pages/TrackUserAsset.svelte";
import AbuseReports from "./pages/AbuseReports.svelte";
import ResolveAsset from "./pages/ResolveAsset.svelte";
import Permissions from "./pages/Permissions.svelte";
import ForceApplication from "./pages/ForceApplication.svelte";
	// import 'bootstrap';

	var pushState = history.pushState;
	history.pushState = function () {
		pushState.apply(history, arguments);
		// timeout is required for mobile devices
		setTimeout(() => {
			window.scrollTo({ top: 0, left: 0, behavior: "auto" });
		}, 5);
	};
</script>

<Router {url}>
	{#await rank.promise then result}
		<Route path="/admin">
			<Index />
		</Route>
		<Route path="/admin/players">
			<Players />
		</Route>
		<Route path="/admin/manage-user/:userId" let:params>
			<ManageUser userId={params.userId} />
		</Route>
		<Route path="/admin/manage-usernames/:userId" let:params>
			<ManageUsernames userId={params.userId} />
		</Route>
		<Route path="/admin/manage-badges-user/:userId" let:params>
			<ManageUserBadges userId={params.userId} />
		</Route>
		<Route path="/admin/manage-robux-user/:userId" let:params>
			<ManageUserRobux userId={params.userId} />
		</Route>
		<Route path="/admin/manage-inventory-user/:userId" let:params>
			<ManageUserInventory userId={params.userId} />
		</Route>
		<Route path="/admin/ban-user/:userId" let:params>
			<BanUser userId={params.userId} />
		</Route>
		<Route path="/admin/permissions">
			<Permissions />
		</Route>
		<Route path="/admin/message-user/:userId" let:params>
			<MessageUser userId={params.userId} />
		</Route>
		<Route path="/admin/user-transactions/:userId" let:params>
			<ManageUserTransactions userId={params.userId} />
		</Route>
		<Route path="/admin/moderation-history/:userId" let:params>
			<UserModerationHistory userId={params.userId} />
		</Route>
		<Route path="/admin/user-trades/:userId" let:params>
			<ManageUserTrades userId={params.userId} />
		</Route>
		<Route path="/admin/manage-user-asset/:userAssetId" let:params>
			<ManageUserAsset userAssetId={parseInt(params.userAssetId, 10)} />
		</Route>
		<Route path="/admin/logs">
			<Logs />
		</Route>
		<Route path="/admin/user/create">
			<CreatePlayer />
		</Route>
		<Route path="/admin/force-application">
			<ForceApplication />
		</Route>
		<Route path="/admin/memos">
			<Memos />
		</Route>
		<Route path="/admin/asset/create-for-item">
			<CreateAssetForItem />
		</Route>
		<Route path="/admin/asset/create">
			<CreateAsset />
		</Route>
		<Route path="/admin/asset/version/create">
			<CreateAssetVersion />
		</Route>
		<Route path="/admin/asset/re-render">
			<AssetReRender />
		</Route>
		<Route path="/admin/product/update">
			<UpdateAssetProduct />
		</Route>
		<Route path="/admin/asset/create/clothing">
			<CreateClothing />
		</Route>
		<Route path="/admin/asset/copy">
			<CopyRobloxClothing />
		</Route>
		<Route path="/admin/asset/approval">
			<AssetApproval />
		</Route>
		<Route path="/admin/lottery">
			<Lottery />
		</Route>
		<Route path="/admin/feature-flags">
			<FeatureFlags />
		</Route>
		<Route path="/admin/applications" let:params>
			<Applications id={undefined} />
		</Route>
		<Route path="/admin/applications/:id" let:params>
			<Applications id={params.id} />
		</Route>
		<Route path="/admin/text-posts">
			<ForumPosts />
		</Route>
		<Route path="/admin/groups">
			<Groups />
		</Route>
		<Route path="/admin/game-history">
			<GameHistory />
		</Route>
		<Route path="/admin/forums">
			<Forums />
		</Route>
		<Route path="/admin/asset/track">
			<TrackUserAsset />
		</Route>
		<Route path="/admin/reports">
			<AbuseReports />
		</Route>
		<Route path="/admin/resolve-url">
			<ResolveAsset />
		</Route>
	{/await}
</Router>


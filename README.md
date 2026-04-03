<div align="center">
  <img src="https://img.shields.io/badge/Status-In--Development-orange?style=for-the-badge" alt="Status">
  <img src="https://img.shields.io/badge/Engine-Unity%206-black?style=for-the-badge&logo=unity" alt="Engine">
  <img src="https://img.shields.io/badge/Course-SE3032-blue?style=for-the-badge" alt="Course">

  <br />

  <h1>🚀 EXODUS PROTOCOL</h1>
  <h3><i>An Interactive 3D Rescue Simulation</i></h3>
  
  <p>
    <b>The city of Neo-Veridia has 4 minutes before orbital bombardment. You are the last extraction unit.</b>
  </p>

  <hr width="50%" />
</div>

<h2>🌌 Project Brief</h2>
<p>
  It is the year 2157. A rogue AI has locked down Neo-Veridia. As <b>Unit ECHO</b>, you must navigate the neon-drenched streets to locate stranded civilians and carry them to safety before the strike hits. Avoid the autonomous hover-patrols—one touch means mission failure.
</p>

<h3>🎮 Core Gameplay</h3>
<table>
  <tr>
    <td width="50%">
      <b>🕹️ Stealth & Movement</b>
      <ul>
        <li>First-person POV exploration.</li>
        <li>Dynamic noise system: Sprinting [Shift] alerts enemies within 6m.</li>
        <li>Crouching [C] reduces visibility but slows you down.</li>
      </ul>
    </td>
    <td width="50%">
      <b>🏗️ Rescue Mechanics</b>
      <ul>
        <li>Physical grab/carry system using [E].</li>
        <li>Deliver humans to the Evacuation Beacon.</li>
        <li>Rescue 5+ humans to win.</li>
      </ul>
    </td>
  </tr>
</table>

<br />

<h2>🤖 Intelligent Systems (Enemy AI)</h2>
<p>
  The hover-patrol fleet utilizes the <b>Unity NavMesh system</b> for autonomous hunting:
</p>
<ul>
  <li><b>Patrol State:</b> Follows pre-defined waypoints through the city grid.</li>
  <li><b>Detection:</b> 45° vision cone with a 12m range.</li>
  <li><b>Dynamic Response:</b> On detection, cars accelerate and bank smoothly into turns to intercept the player.</li>
  <li><b>Search Logic:</b> If line-of-sight is lost, agents search the last known position for 5 seconds.</li>
</ul>

<br />

<h2>👥 The Development Team</h2>
<table width="100%">
  <thead>
    <tr align="left">
      <th>Role</th>
      <th>Graphics Duty</th>
      <th>Technical Deliverable</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><b>World Builder</b></td>
      <td>Level Design & Lighting</td>
      <td>Neo-Veridia layout, baked GI, and cyberpunk textures.</td>
    </tr>
    <tr>
      <td><b>Systems Engineer</b></td>
      <td>Physics & Interaction</td>
      <td>Grab/Carry mechanics and proximity collision triggers.</td>
    </tr>
    <tr>
      <td><b>Core Developer</b></td>
      <td>3D Modeling</td>
      <td>Custom Hover-Car and Human NPC assets (Blender/Maya).</td>
    </tr>
    <tr>
      <td><b>Agent Controller</b></td>
      <td>AI & Animation</td>
      <td>State machines, NavMesh logic, and banking animations.</td>
    </tr>
  </tbody>
</table>

<br />

<h2>🛠️ Technical Specs</h2>
<details>
  <summary><b>Click to expand Technical Details</b></summary>
  <br />
  <ul>
    <li><b>Optimization:</b> Target ≤ 8,000 triangles for vehicles; ≤ 5,000 for humans.</li>
    <li><b>Lighting:</b> Hybrid setup using baked lightmaps and real-time volumetric neon shafts.</li>
    <li><b>Environment:</b> 300m x 300m map with 3 distinct districts (Commercial, Residential, Industrial).</li>
    <li><b>Git Workflow:</b> Asset naming convention <code>[Role]_[AssetName]_[Version]</code>.</li>
  </ul>
</details>

<br />

<h2>📅 Project Roadmap</h2>
<p>
  <b>Week 11:</b> Concept & Scope Pitch <br />
  <b>Week 13:</b> Logic & Asset Prototype <br />
  <b>Week 15:</b> Final Showcase & Viva (May 3rd, 2026)
</p>

<hr />

<div align="center">
  <p><i>BSc (Hons) in Computer Science | SE3032 – Graphics and Visualization</i></p>
</div>
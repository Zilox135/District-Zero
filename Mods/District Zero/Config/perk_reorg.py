#!/usr/bin/env python3
import re

with open('progression.xml', 'r', encoding='utf-8') as f:
    content = f.read()

print('Starting perk reorganization...')

# Helper function for simple replacements
def rep(old, new):
    global content
    if old in content:
        content = content.replace(old, new)
        return True
    return False

# 1. INFILTRATOR: Cosmotech -> Neonaut
rep('<!-- *** INFILTRATOR -->', '<!-- *** INFILTRATOR (Moved to Neonaut) -->')
rep('>attCosmotech</setattribute>\n<set xpath="//perk[@name=\'perkInfiltrator\']',
    '>attNeonaut</setattribute>\n<set xpath="//perk[@name=\'perkInfiltrator\']')
rep('>attCosmotech</set>\n\n<set xpath="//perk[@name=\'perkInfiltrator\']/level_requirements',
    '>attNeonaut</set>\n\n<set xpath="//perk[@name=\'perkInfiltrator\']/level_requirements')
rep("desc_key\">reqCosmotechLevel01</set>\n<set xpath=\"//perk[@name='perkInfiltrator']/level_requirements[@level='2']",
    "desc_key\">reqNeonautLevel01</set>\n<set xpath=\"//perk[@name='perkInfiltrator']/level_requirements[@level='2']")
rep("desc_key\">reqCosmotechLevel03</set>\n<set xpath=\"//perk[@name='perkInfiltrator']/level_requirements[@level='3']",
    "desc_key\">reqNeonautLevel03</set>\n<set xpath=\"//perk[@name='perkInfiltrator']/level_requirements[@level='3']")
rep("desc_key\">reqCosmotechLevel05</set>\n\n\t<!-- *** ANIMAL",
    "desc_key\">reqNeonautLevel05</set>\n\n\t<!-- *** ANIMAL")
print('  [1/8] Infiltrator: Cosmotech -> Neonaut')

# 2. BRAWLER: Cybernetician -> Exomedic
rep('<!-- *** BRAWLER -->', '<!-- *** BRAWLER (Moved to Exomedic) -->')
rep('>attCybernetician</setattribute>\n<set xpath="//perk[@name=\'perkBrawler\']',
    '>attExomedic</setattribute>\n<set xpath="//perk[@name=\'perkBrawler\']')
rep('>attCybernetician</set>\n\n<set xpath="//perk[@name=\'perkBrawler\']/level_requirements',
    '>attExomedic</set>\n\n<set xpath="//perk[@name=\'perkBrawler\']/level_requirements')
for i in range(1, 6):
    rep(f"desc_key\">reqCyberneticianLevel0{i}</set>\n<set xpath=\"//perk[@name='perkBrawler']",
        f"desc_key\">reqExomedicLevel0{i}</set>\n<set xpath=\"//perk[@name='perkBrawler']")
rep("desc_key\">reqCyberneticianLevel05</set>\n\n\t<!-- *** MACHINE",
    "desc_key\">reqExomedicLevel05</set>\n\n\t<!-- *** MACHINE")
print('  [2/8] Brawler: Cybernetician -> Exomedic')

# 3. PAIN_TOLERANCE: Empath -> Exomedic
rep('<!-- *** PAIN_TOLERANCE -->', '<!-- *** PAIN_TOLERANCE (Moved to Exomedic) -->')
rep('>attEmpath</setattribute>\n<set xpath="//perk[@name=\'perkPainTolerance\']',
    '>attExomedic</setattribute>\n<set xpath="//perk[@name=\'perkPainTolerance\']')
rep('>attEmpath</set>\n\n<set xpath="//perk[@name=\'perkPainTolerance\']/level_requirements',
    '>attExomedic</set>\n\n<set xpath="//perk[@name=\'perkPainTolerance\']/level_requirements')
for i in range(1, 6):
    rep(f"desc_key\">reqEmpathLevel0{i}</set>\n<set xpath=\"//perk[@name='perkPainTolerance']",
        f"desc_key\">reqExomedicLevel0{i}</set>\n<set xpath=\"//perk[@name='perkPainTolerance']")
rep("desc_key\">reqEmpathLevel05</set>\n\n\t<!-- *** HEALING",
    "desc_key\">reqExomedicLevel05</set>\n\n\t<!-- *** HEALING")
print('  [3/8] Pain Tolerance: Empath -> Exomedic')

# 4. RUN_AND_GUN: Cybernetician -> Exomedic
rep('<!-- *** RUN_AND_GUN -->', '<!-- *** RUN_AND_GUN (Moved to Exomedic) -->')
rep('>attCybernetician</setattribute>\n<set xpath="//perk[@name=\'perkRunAndGun\']',
    '>attExomedic</setattribute>\n<set xpath="//perk[@name=\'perkRunAndGun\']')
rep('>attCybernetician</set>\n\n<set xpath="//perk[@name=\'perkRunAndGun\']/level_requirements',
    '>attExomedic</set>\n\n<set xpath="//perk[@name=\'perkRunAndGun\']/level_requirements')
rep("desc_key\">reqCyberneticianLevel01</set>\n<set xpath=\"//perk[@name='perkRunAndGun']",
    "desc_key\">reqExomedicLevel01</set>\n<set xpath=\"//perk[@name='perkRunAndGun']")
rep("desc_key\">reqCyberneticianLevel03</set>\n<set xpath=\"//perk[@name='perkRunAndGun']",
    "desc_key\">reqExomedicLevel03</set>\n<set xpath=\"//perk[@name='perkRunAndGun']")
rep("desc_key\">reqCyberneticianLevel05</set>\n\n\t<!-- *** FLURRY_OF_BLOWS",
    "desc_key\">reqExomedicLevel05</set>\n\n\t<!-- *** FLURRY_OF_BLOWS")
print('  [4/8] Run And Gun: Cybernetician -> Exomedic')

# 5. LOCK_PICKING: Cybernetician -> Neonaut
rep('<!-- *** LOCK_PICKING -->', '<!-- *** LOCK_PICKING (Moved to Neonaut) -->')
rep('>attCybernetician</setattribute>\n\n<!-- Add level_requirements',
    '>attNeonaut</setattribute>\n\n<!-- Add level_requirements')
rep('progression_name="attCybernetician" operation="GTE" value="1" desc_key="reqCyberneticianLevel01"',
    'progression_name="attNeonaut" operation="GTE" value="1" desc_key="reqNeonautLevel01"')
rep('progression_name="attCybernetician" operation="GTE" value="3" desc_key="reqCyberneticianLevel03"',
    'progression_name="attNeonaut" operation="GTE" value="3" desc_key="reqNeonautLevel03"')
rep('progression_name="attCybernetician" operation="GTE" value="5" desc_key="reqCyberneticianLevel05"',
    'progression_name="attNeonaut" operation="GTE" value="5" desc_key="reqNeonautLevel05"')
print('  [5/8] Lock Picking: Cybernetician -> Neonaut')

# 6. PERCEPTION_MASTERY: Cosmotech -> Neonaut
rep('<!-- perkPerceptionMastery -> attCosmotech (salvaging, rifles theme) -->',
    '<!-- perkPerceptionMastery -> attNeonaut (Moved - awareness = exploration) -->')
rep('>attCosmotech</setattribute>\n<remove xpath="//perk[@name=\'perkPerceptionMastery\']',
    '>attNeonaut</setattribute>\n<remove xpath="//perk[@name=\'perkPerceptionMastery\']')
# Replace inline level requirements for PerceptionMastery
content = re.sub(
    r'(perkPerceptionMastery.*?)(progression_name=")attCosmotech(" operation="GTE" value="\d+" desc_key=")reqCosmotechLevel',
    r'\1\2attNeonaut\3reqNeonautLevel', content, flags=re.DOTALL, count=5)
print('  [6/8] Perception Mastery: Cosmotech -> Neonaut')

# 7. FORTITUDE_MASTERY: Empath -> Exomedic
rep('<!-- perkFortitudeMastery -> attEmpath (resilience, recovery theme) -->',
    '<!-- perkFortitudeMastery -> attExomedic (Moved - physical = fitness) -->')
rep('>attEmpath</setattribute>\n<remove xpath="//perk[@name=\'perkFortitudeMastery\']',
    '>attExomedic</setattribute>\n<remove xpath="//perk[@name=\'perkFortitudeMastery\']')
content = re.sub(
    r'(perkFortitudeMastery.*?)(progression_name=")attEmpath(" operation="GTE" value="\d+" desc_key=")reqEmpathLevel',
    r'\1\2attExomedic\3reqExomedicLevel', content, flags=re.DOTALL, count=5)
print('  [7/8] Fortitude Mastery: Empath -> Exomedic')

# 8. SIPHONING_STRIKES: Empath -> Exomedic
rep('<!-- perkSiphoningStrikes -> attEmpath (healing/recovery theme) -->',
    '<!-- perkSiphoningStrikes -> attExomedic (Moved - combat recovery) -->')
rep('>attEmpath</setattribute>\n<remove xpath="//perk[@name=\'perkSiphoningStrikes\']',
    '>attExomedic</setattribute>\n<remove xpath="//perk[@name=\'perkSiphoningStrikes\']')
content = re.sub(
    r'(perkSiphoningStrikes.*?)(progression_name=")attEmpath(" operation="GTE" value="\d+" desc_key=")reqEmpathLevel',
    r'\1\2attExomedic\3reqExomedicLevel', content, flags=re.DOTALL, count=5)
print('  [8/8] Siphoning Strikes: Empath -> Exomedic')

with open('progression.xml', 'w', encoding='utf-8') as f:
    f.write(content)

print('\nDone! All 8 perks reorganized.')
print('Summary:')
print('  Exomedic gains: Brawler, Pain Tolerance, Run And Gun, Fortitude Mastery, Siphoning Strikes')
print('  Neonaut gains: Infiltrator, Lock Picking, Perception Mastery')

